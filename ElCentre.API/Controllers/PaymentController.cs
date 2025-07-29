using EcommerceGraduation.API.Helper;
using ElCentre.API.Helper;
using ElCentre.Core.Entities;
using ElCentre.Core.Interfaces;
using ElCentre.Core.Services;
using ElCentre.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

namespace ElCentre.API.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class PaymentController : ControllerBase
    {
        private readonly IPaymobService _paymobService;
        private readonly IUnitofWork _work;
        private readonly IConfiguration _configuration;
        private readonly ElCentreDbContext _context;
        private readonly ICouponService _couponService;

        public PaymentController(IPaymobService paymobService, IUnitofWork work, IConfiguration configuration, ElCentreDbContext context, ICouponService couponService)
        {
            _paymobService = paymobService;
            _work = work;
            _configuration = configuration;
            _context = context;
            _couponService = couponService;
        }

        /// <summary>
        /// Creates a payment token for a course enrollment.
        /// </summary>
        /// <param name="courseID">The course ID to enroll in</param>
        /// <param name="paymentMethod">The payment method (card, wallet)</param>
        /// <returns>The redirect URL for payment processing</returns>
        [Authorize]
        [HttpPost("create-payment-token")]
        public async Task<IActionResult> CreatePaymentToken(
            [FromQuery] int courseID,
            [FromQuery] string paymentMethod,
            [FromQuery] string? couponCode)
        {
            if (courseID <= 0)
                return BadRequest("Invalid course ID.");

            var studentId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(studentId))
                return Unauthorized("User not authenticated.");

            var enrollment = await _work.EnrollmentRepository.AddEnrollmentAsync(courseID, studentId);
            if (enrollment == null)
                return NotFound("Enrollment not found.");

            try
            {
                var amount = enrollment.Course.Price;
                decimal totalAmount = amount; // Initialize totalAmount with the default value of amount.

                if (!string.IsNullOrEmpty(couponCode))
                {
                    totalAmount = await _couponService.ApplyCouponAsync(couponCode, amount, studentId, courseID);
                    if (totalAmount == 0)
                    {
                        enrollment.PaymentStatus = "Success"; // Mark as free enrollment
                        await _work.EnrollmentRepository.UpdateAsync(enrollment);
                        return Ok(new APIResponse(200, "Student enrolled successfully"));
                    }
                }

                if (string.IsNullOrWhiteSpace(paymentMethod))
                    return BadRequest("Payment method is required.");

                if (paymentMethod.Equals("card", StringComparison.OrdinalIgnoreCase) ||
                    paymentMethod.Equals("wallet", StringComparison.OrdinalIgnoreCase))
                {
                    var (enrollmentResult, redirectUrl) = await _paymobService.ProcessPaymentAsync(enrollment.Id, paymentMethod, totalAmount, couponCode);
                    return Ok(new { RedirectUrl = redirectUrl });
                }
                else
                {
                    return BadRequest("Invalid payment method. Supported methods are 'card' and 'wallet'.");
                }
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Error processing payment: {ex.Message}");
            }
        }

        /// <summary>
        /// Handles the user redirect callback after payment.
        /// Displays success or failure page.
        /// </summary>
        [HttpGet("callback")]
        public async Task<IActionResult> CallbackAsync()
        {
            var query = Request.Query;

            string[] fields = new[]
            {
                "amount_cents", "created_at", "currency", "error_occured", "has_parent_transaction",
                "id", "integration_id", "is_3d_secure", "is_auth", "is_capture", "is_refunded",
                "is_standalone_payment", "is_voided", "order", "owner", "pending",
                "source_data.pan", "source_data.sub_type", "source_data.type", "success"
            };

            var concatenated = new StringBuilder();
            foreach (var field in fields)
            {
                if (query.TryGetValue(field, out var value))
                {
                    concatenated.Append(value);
                }
                else
                {
                    return BadRequest($"Missing expected field: {field}");
                }
            }

            string receivedHmac = query["hmac"];
            string calculatedHmac = _paymobService.ComputeHmacSHA512(concatenated.ToString(), _configuration["Paymob:HMAC"]);

            if (receivedHmac.Equals(calculatedHmac, StringComparison.OrdinalIgnoreCase))
            {
                bool.TryParse(query["success"], out bool isSuccess);
                var specialReference = query["merchant_order_id"];

                if (isSuccess)
                {
                    await _paymobService.UpdateOrderSuccess(specialReference);
                    return Content(HtmlGenerator.GenerateSuccessHtml(), "text/html");
                }

                await _paymobService.UpdateOrderFailed(specialReference);
                return Content(HtmlGenerator.GenerateFailedHtml(), "text/html");
            }

            return Content(HtmlGenerator.GenerateSecurityHtml(), "text/html");
        }

        /// <summary>
        /// Handles Paymob server-to-server callback (processed transaction).
        /// This ensures DB updates even if user never returns.
        /// </summary>
        [HttpPost("server-callback")]
        public async Task<IActionResult> ServerCallback([FromBody] JsonElement payload)
        {
            try
            {
                string receivedHmac = Request.Query["hmac"];
                string secret = _configuration["Paymob:HMAC"];

                string[] keys = {
                    "amount_cents","created_at","currency","error_occured","has_parent_transaction",
                    "id","integration_id","is_3d_secure","is_auth","is_capture","is_refunded",
                    "is_standalone_payment","is_voided","order.id","owner","pending",
                    "source_data.pan","source_data.sub_type","source_data.type","success"
                };

                var obj = payload.GetProperty("obj");
                var sb = new StringBuilder();
                foreach (var key in keys)
                {
                    var parts = key.Split('.');
                    JsonElement value = obj;
                    foreach (var part in parts)
                    {
                        if (value.TryGetProperty(part, out var temp))
                            value = temp;
                        else
                            value = default;
                    }
                    sb.Append(value.ToString());
                }

                string calculatedHmac = _paymobService.ComputeHmacSHA512(sb.ToString(), secret);

                if (!receivedHmac.Equals(calculatedHmac, StringComparison.OrdinalIgnoreCase))
                    return Unauthorized("Invalid HMAC");

                string merchantOrderId = obj.GetProperty("order").GetProperty("merchant_order_id").GetString();
                bool isSuccess = obj.GetProperty("success").GetBoolean();

                if (isSuccess)
                    await _paymobService.UpdateOrderSuccess(merchantOrderId);
                else
                    await _paymobService.UpdateOrderFailed(merchantOrderId);

                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error processing server callback: {ex.Message}");
            }
        }
    }
}
