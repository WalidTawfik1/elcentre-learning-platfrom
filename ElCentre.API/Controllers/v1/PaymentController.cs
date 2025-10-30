using Asp.Versioning;
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
using Microsoft.Extensions.Logging;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

namespace ElCentre.API.Controllers.v1
{
    [ApiController]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiVersion("1.0")]
    public class PaymentController : ControllerBase
    {
        private readonly IPaymobService _paymobService;
        private readonly IUnitofWork _work;
        private readonly IConfiguration _configuration;
        private readonly ElCentreDbContext _context;
        private readonly ICouponService _couponService;
        private readonly ILogger<PaymentController> _logger;

        public PaymentController(
            IPaymobService paymobService,
            IUnitofWork work,
            IConfiguration configuration,
            ElCentreDbContext context,
            ICouponService couponService,
            ILogger<PaymentController> logger)
        {
            _paymobService = paymobService;
            _work = work;
            _configuration = configuration;
            _context = context;
            _couponService = couponService;
            _logger = logger;
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
                    return Content(HtmlGenerator.GenerateSuccessHtml(), "text/html");
                }

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

                if (!payload.TryGetProperty("obj", out var obj))
                    return BadRequest("Missing 'obj' in payload.");

                string[] fields = new[]
                {
                    "amount_cents", "created_at", "currency", "error_occured", "has_parent_transaction",
                    "id", "integration_id", "is_3d_secure", "is_auth", "is_capture", "is_refunded",
                    "is_standalone_payment", "is_voided", "order.id", "owner", "pending",
                    "source_data.pan", "source_data.sub_type", "source_data.type", "success"
                };

                var concatenated = new StringBuilder();
                foreach (var field in fields)
                {
                    string[] parts = field.Split('.');
                    JsonElement current = obj;
                    bool found = true;
                    foreach (var part in parts)
                    {
                        if (current.ValueKind == JsonValueKind.Object && current.TryGetProperty(part, out var next))
                            current = next;
                        else
                        {
                            found = false;
                            break;
                        }
                    }

                    if (!found || current.ValueKind == JsonValueKind.Null)
                    {
                        concatenated.Append(""); // Use empty string for missing/null fields
                    }
                    else if (current.ValueKind == JsonValueKind.True || current.ValueKind == JsonValueKind.False)
                    {
                        concatenated.Append(current.GetBoolean() ? "true" : "false"); // Lowercase boolean
                    }
                    else
                    {
                        concatenated.Append(current.ToString());
                    }
                }

                string calculatedHmac = _paymobService.ComputeHmacSHA512(concatenated.ToString(), secret);

                if (!receivedHmac.Equals(calculatedHmac, StringComparison.OrdinalIgnoreCase))
                    return Unauthorized("Invalid HMAC");

                string merchantOrderId = null;
                if (obj.TryGetProperty("order", out var order) &&
                    order.TryGetProperty("merchant_order_id", out var merchantOrderIdElement) &&
                    merchantOrderIdElement.ValueKind != JsonValueKind.Null)
                {
                    merchantOrderId = merchantOrderIdElement.ToString();
                }

                bool isSuccess = obj.TryGetProperty("success", out var successElement) && successElement.GetBoolean();

                if (!string.IsNullOrEmpty(merchantOrderId))
                {
                    if (isSuccess)
                        await _paymobService.UpdateOrderSuccess(merchantOrderId);
                    else
                        await _paymobService.UpdateOrderFailed(merchantOrderId);
                }

                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error processing server callback: {ex.Message}");
            }
        }
    }
}