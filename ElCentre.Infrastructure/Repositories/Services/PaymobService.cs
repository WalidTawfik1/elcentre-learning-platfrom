using ElCentre.Core.DTO;
using ElCentre.Core.Entities;
using ElCentre.Core.Interfaces;
using ElCentre.Core.Services;
using ElCentre.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Org.BouncyCastle.Crypto.Macs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using X.Paymob.CashIn;
using X.Paymob.CashIn.Models.Orders;
using X.Paymob.CashIn.Models.Payment;

namespace ElCentre.Infrastructure.Repositories.Services
{
    public class PaymobService : IPaymobService
    {
        private readonly IUnitofWork _unitofWork;
        private readonly IConfiguration _configuration;
        private readonly ElCentreDbContext _context;
        private readonly IPaymobCashInBroker _broker;
        private readonly IEmailService _emailService;

        public PaymobService(
            ElCentreDbContext context,
            IConfiguration configuration,
            IUnitofWork unitofWork,
            IPaymobCashInBroker broker,
            IEmailService emailService)
        {
            _context = context;
            _configuration = configuration;
            _unitofWork = unitofWork;
            _broker = broker;
            _emailService = emailService;
        }

        public string ComputeHmacSHA512(string data, string secret)
        {
            var keyBytes = Encoding.UTF8.GetBytes(secret);
            var dataBytes = Encoding.UTF8.GetBytes(data);

            using (var hmac = new HMACSHA512(keyBytes))
            {
                var hash = hmac.ComputeHash(dataBytes);
                return BitConverter.ToString(hash).Replace("-", "").ToLower();
            }
        }

        public string GetPaymentIframeUrl(string paymentToken)
        {
            if (string.IsNullOrEmpty(paymentToken))
            {
                throw new ArgumentNullException(nameof(paymentToken), "Payment token cannot be null or empty.");
            }

            string iframeId = _configuration["Paymob:IframeId"];
            if (string.IsNullOrEmpty(iframeId))
            {
                throw new InvalidOperationException("Paymob iframe ID is not configured.");
            }

            // Build the Paymob iframe URL
            string iframeUrl = $"https://accept.paymob.com/api/acceptance/iframes/{iframeId}?payment_token={paymentToken}";
            return iframeUrl;
        }

        public async Task<(Enrollment Enrollment, string RedirectUrl)> ProcessPaymentForOrderAsync(int enrollmentId, string paymentMethod)
        {
            var enrollment = await _context.Enrollments
                .Include(e => e.Payments)
                .Include(e => e.Student)
                .Include(e => e.Course)
                .FirstOrDefaultAsync(e => e.Id == enrollmentId);

            if (enrollment == null)
            {
                throw new KeyNotFoundException($"Enrollment with ID {enrollmentId} not found.");
            }

            var student = enrollment.Student;
            if (student == null)
            {
                throw new InvalidOperationException("Student associated with the enrollment not found.");
            }

            var amountCents = (int)enrollment.Course.Price * 100;

            var orderRequest = CashInCreateOrderRequest.CreateOrder(amountCents);
            var orderResponse = await _broker.CreateOrderAsync(orderRequest);

            string firstName = student.FirstName ?? "Guest";
            string lastName = student.LastName ?? "User";

            var billingData = new CashInBillingData(
                firstName: firstName,
                lastName: lastName,
                email: student.Email,
                phoneNumber: student.PhoneNumber,
                country: "Egypt"
            );

            var integrationId = int.Parse(DetermineIntegrationId(paymentMethod));

            var paymentKeyRequest = new CashInPaymentKeyRequest
            (
                integrationId: integrationId,
                orderId: orderResponse.Id,
                billingData: billingData,
                amountCents: amountCents,
                currency: "EGP",
                lockOrderWhenPaid: true,
                expiration: 3600 // 1 hour expiration
            );

            var paymentKeyResponse = await _broker.RequestPaymentKeyAsync(paymentKeyRequest);

            var payment = new Payment
            {
                Amount = enrollment.Course.Price,
                PaymentMethod = paymentMethod,
                Status = "Pending",
                TransactionId = orderResponse.Id.ToString(),
                EnrollmentId = enrollment.Id,
                PaymentDate = DateTime.Now,
                UserId = student.Id
            };
            _context.Payments.Add(payment);

            enrollment.PaymentStatus = "Pending";
            await _context.SaveChangesAsync();

            // Determine redirect URL based on payment method
            string redirectUrl;
            if (paymentMethod?.ToLower() == "wallet")
            {
                // For mobile wallet, we need to use a different flow
                redirectUrl = GetMobileWalletPaymentUrl(paymentKeyResponse.PaymentKey, student.PhoneNumber);
            }
            else
            {
                // For card payments, use the iframe approach
                redirectUrl = GetPaymentIframeUrl(paymentKeyResponse.PaymentKey);
            }

            return (enrollment, redirectUrl);
        }


        public string GetMobileWalletPaymentUrl(string paymentToken, string phoneNumber)
        {
            if (string.IsNullOrEmpty(paymentToken))
            {
                throw new ArgumentNullException(nameof(paymentToken), "Payment token cannot be null or empty.");
            }

            if (string.IsNullOrEmpty(phoneNumber))
            {
                throw new ArgumentNullException(nameof(phoneNumber), "Phone number is required for mobile wallet payments.");
            }

            // Format the phone number if needed (remove spaces, country code handling, etc.)
            phoneNumber = FormatPhoneNumber(phoneNumber);

            // Request the wallet payment URL from Paymob API
            return $"https://accept.paymob.com/api/acceptance/payments/pay?payment_token={paymentToken}&source=wallet&wallet_mobile_number={phoneNumber}";
        }

        private string FormatPhoneNumber(string phoneNumber)
        {
            // Remove any non-digit characters
            phoneNumber = new string(phoneNumber.Where(char.IsDigit).ToArray());

            // Ensure Egyptian format (if needed)
            if (phoneNumber.StartsWith("2") && phoneNumber.Length == 12)
            {
                return phoneNumber; // Already in correct format
            }
            else if (!phoneNumber.StartsWith("2") && phoneNumber.Length == 11)
            {
                return "2" + phoneNumber; // Add country code
            }

            return phoneNumber;
        }


        public async Task<Enrollment> UpdateOrderSuccess(string paymentIntentId)
        {
            var payment = await _context.Payments
                .Include(p => p.Enrollment)
                .FirstOrDefaultAsync(p => p.TransactionId == paymentIntentId);

            if (payment == null)
            {
                throw new KeyNotFoundException($"Payment with transaction ID {paymentIntentId} not found.");
            }

            var enrollment = await _context.Enrollments
                .Include(e => e.Student)
                .FirstOrDefaultAsync(e => e.Id == payment.EnrollmentId);

            if (enrollment == null)
            {
                throw new KeyNotFoundException($"Enrollment with ID {payment.EnrollmentId} not found.");
            }

            // Update enrollment status and payment status
            enrollment.PaymentStatus = "Success";
            payment.Status = "Success";
            
            await _context.SaveChangesAsync();

            // Send confirmation email
            await SendPaymentConfirmationEmail(payment);

            return payment.Enrollment;
        }

        public async Task<Enrollment> UpdateOrderFailed(string paymentIntentId)
        {
            var payment = await _context.Payments
                .Include(p => p.Enrollment)
                .FirstOrDefaultAsync(p => p.TransactionId == paymentIntentId);

            if (payment == null)
            {
                throw new KeyNotFoundException($"Payment with transaction ID {paymentIntentId} not found.");
            }

            var enrollment = await _context.Enrollments
                .Include(e => e.Student)
                .FirstOrDefaultAsync(e => e.Id == payment.EnrollmentId);

            if (enrollment == null)
            {
                throw new KeyNotFoundException($"Enrollment with ID {payment.EnrollmentId} not found.");
            }

            // Update enrollment status and payment status
            enrollment.PaymentStatus = "Failed";
            payment.Status = "Failed";

            await _context.SaveChangesAsync();

            return payment.Enrollment;
        }

        private async Task SendPaymentConfirmationEmail(Payment payment)
        {
            var student = await _context.Users.FindAsync(payment.UserId);
            var course = await _context.Courses.FindAsync(payment.Enrollment.CourseId);

            if (student != null && course != null)
            {
                var emailContent = $@"
                        <html>
                    <head>
                        <style>
                            body {{
                                font-family: Arial, sans-serif;
                                color: #333333;
                            }}
                            .container {{
                                padding: 20px;
                            }}
                            .footer {{
                                margin-top: 30px;
                                font-size: 14px;
                                color: #777777;
                            }}
                        </style>
                    </head>
                    <body>
                        <div class='container'>
                            <p>Dear {student.FirstName},</p>
                    
                            <p>We are pleased to inform you that your payment of <strong>{payment.Amount} EGP</strong> for the course titled <em>'{course.Title}'</em> has been successfully processed.</p>
                    
                            <p>Thank you for choosing ElCentre for your learning journey. We wish you great success with your course!</p>
                    
                            <div class='footer'>
                                <p>Best regards,<br>The ElCentre Team</p>
                            </div>
                        </div>
                    </body>
                    </html>";

                var emailDTO = new EmailDTO(
                    to: student.Email,
                    from: "elcentre.business@gmail.com",
                    subject: "Payment Confirmation",
                    content: emailContent);

                await _emailService.SendEmailAsync(emailDTO);
            }
        }


        private string DetermineIntegrationId(string paymentMethod)
        {
            return paymentMethod?.ToLower() switch
            {
                "card" => _configuration["Paymob:CardIntegrationId"] ?? throw new ArgumentException("Card integration ID not configured"),
                "wallet" => _configuration["Paymob:MobileIntegrationId"] ?? throw new ArgumentException("Wallet integration ID not configured"),
                _ => throw new ArgumentException($"Invalid payment method: {paymentMethod}")
            };
        }
    }
}
