using EcommerceGraduation.API.Helper;
using ElCentre.Core.Interfaces;
using ElCentre.Core.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using System.Security.Claims;
using System.Text;

namespace ElCentre.API.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class PaymentController : ControllerBase
    {
        private readonly IPaymobService _paymobService;
        private readonly IUnitofWork _work;
        private readonly IConfiguration _configuration;
        public PaymentController(IPaymobService paymobService, IUnitofWork work, IConfiguration configuration)
        {
            _paymobService = paymobService;
            _work = work;
            _configuration = configuration;
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
            [FromQuery] string paymentMethod)
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
                if(paymentMethod.Equals("card", StringComparison.OrdinalIgnoreCase))
                {
                    // Process payment for card method
                    var (enrollmentResult, redirectUrl) = await _paymobService.ProcessPaymentForCardAsync(enrollment.Id);
                    return Ok(new { RedirectUrl = redirectUrl });
                }
                else if (paymentMethod.Equals("wallet", StringComparison.OrdinalIgnoreCase))
                {
                    // Process payment for wallet method
                    var (enrollmentResult, redirectUrl) = await _paymobService.ProcessPaymentForWalletAsync(enrollment.Id);
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
        /// Handles the Paymob callback after payment processing.
        /// </summary>
        /// <returns></returns>
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

                var specialReference = query["merchant_order_id"].ToString() ?? query["order"].ToString();
                    
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
    }
}
