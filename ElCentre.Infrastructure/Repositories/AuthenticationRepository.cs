using ElCentre.Core.DTO;
using ElCentre.Core.Entities;
using ElCentre.Core.Interfaces;
using ElCentre.Core.Services;
using ElCentre.Core.Sharing;
using ElCentre.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElCentre.Infrastructure.Repositories
{
    public class AuthenticationRepository : IAuthentication
    {
        private readonly UserManager<AppUser> userManager;
        private readonly IEmailService emailService;
        private readonly SignInManager<AppUser> signInManager;
        private readonly IGenerateToken generateToken;
        private readonly ElCentreDbContext context;

        public AuthenticationRepository(UserManager<AppUser> userManager, IEmailService emailService, SignInManager<AppUser> signInManager, IGenerateToken generateToken, ElCentreDbContext context)
        {
            this.userManager = userManager;
            this.emailService = emailService;
            this.signInManager = signInManager;
            this.generateToken = generateToken;
            this.context = context;
        }

        public async Task<string> RegisterAsync(RegisterDTO registerDTO)
        {
            try
            {
                if (registerDTO == null)
                {
                    return null;
                }

                // Add email format validation
                if (string.IsNullOrWhiteSpace(registerDTO.Email) || !IsValidEmail(registerDTO.Email))
                {
                    return "Please enter a valid email address";
                }

                var existUser = await userManager.FindByEmailAsync(registerDTO.Email);

                if (existUser != null && existUser.IsDeleted)
                {
                    return "This account has been deleted. Please contact elcentre.business@gmail.com for assistance.";
                }

                if (existUser != null)
                {
                    return "This email already exists";
                }

                var user = new AppUser
                {
                    FirstName = registerDTO.FirstName,
                    LastName = registerDTO.LastName,
                    Email = registerDTO.Email,
                    UserName = registerDTO.Email.Split('@')[0],
                    PhoneNumber = registerDTO.PhoneNumber,
                    CreatedAt = DateTime.Now,
                    Gender = registerDTO.Gender,
                    DateOfBirth = registerDTO.DateOfBirth,
                    UserType = registerDTO.UserType,
                    Country = registerDTO.Country
                };
                var result = await userManager.CreateAsync(user, registerDTO.Password);
                if (!result.Succeeded)
                {
                    return string.Join(", ", result.Errors.Select(e => e.Description));
                }
                if (registerDTO.UserType == "Student")
                {
                    var roleResult = await userManager.AddToRoleAsync(user, "Student");
                    if (!roleResult.Succeeded)
                    {
                        return string.Join(", ", roleResult.Errors.Select(e => e.Description));
                    }
                }
                else if (registerDTO.UserType == "Instructor")
                {
                    var roleResult = await userManager.AddToRoleAsync(user, "Instructor");
                    if (!roleResult.Succeeded)
                    {
                        return string.Join(", ", roleResult.Errors.Select(e => e.Description));
                    }
                }
                else if (registerDTO.UserType == "Admin")
                {
                    var roleResult = await userManager.AddToRoleAsync(user, "Admin");
                    if (!roleResult.Succeeded)
                    {
                        return string.Join(", ", roleResult.Errors.Select(e => e.Description));
                    }
                }
                else
                {
                    return "Invalid User Type";

                }

                // Generate OTP code instead of token
                string otpCode = GenerateCode.GenerateOtpCode(6);

                // Store OTP in database
                await StoreOtpForUser(user.Email, otpCode);

                // Send OTP email
                await SendEmailWithOtp(user.Email, otpCode, "Please verify your email by entering the code below in the app");

                return "User Created Successfully";
            }
            catch (DbUpdateException dbEx)
            {
                var innerMessage = dbEx.InnerException?.Message ?? "No inner exception";
                // Log or return the innerMessage
                return $"Database error: {innerMessage}";
            }
            catch (Exception ex)
            {
                // Log or return ex.Message
                return $"Unexpected error: {ex.Message}";
            }
        }

        public async Task<string> LoginAsync(LoginDTO loginDTO)
        {
            if (loginDTO == null)
            {
                return null;
            }

            // Add email format validation
            if (string.IsNullOrWhiteSpace(loginDTO.Email) || !IsValidEmail(loginDTO.Email))
            {
                return "Please enter a valid email address";
            }

            var user = await userManager.FindByEmailAsync(loginDTO.Email);

            if (user == null)
            {
                return "Invalid Email or Password";
            }

            if (user.IsDeleted)
            {
                return "This account has been deleted. Please contact elcentre.business@gmail.com for assistance.";
            }

            if (!user.EmailConfirmed)
            {
                // Generate OTP code instead of token
                string otpCode = GenerateCode.GenerateOtpCode(6);

                // Store OTP in database
                await StoreOtpForUser(user.Email, otpCode);

                // Send OTP email
                await SendEmailWithOtp(user.Email, otpCode, "Please verify your email by entering the code below in the app");

                return "Please verify your email first. We have sent a verification code to your email";
            }

            var result = await signInManager.CheckPasswordSignInAsync(user, loginDTO.Password, true);
            if (result.Succeeded)
            {
                return generateToken.GetAndCreateToken(user);
            }
            return "Invalid Email or Password";
        }

        public async Task<bool> SendEmailForgetPassword(string email)
        {
            var user = await userManager.FindByEmailAsync(email);
            if (user == null)
            {
                return false;
            }

            // Generate OTP code instead of token
            string otpCode = GenerateCode.GenerateOtpCode(6);

            // Store OTP in database with purpose "ResetPassword"
            await StoreOtpForUser(user.Email, otpCode, "ResetPassword");

            // Send OTP email
            await SendEmailWithOtp(user.Email, otpCode, "Please use the code below to reset your password");

            return true;
        }

        public async Task<string> ResetPassword(ResetPasswordDTO resetPasswordDTO)
        {
            var user = await userManager.FindByEmailAsync(resetPasswordDTO.Email);
            if (user == null)
            {
                return "Invalid Email";
            }

            // Verify OTP code
            var otpVerification = await context.OtpVerifications
                .Where(o => o.Email == resetPasswordDTO.Email && o.OtpCode == resetPasswordDTO.Code)
                .OrderByDescending(o => o.ExpirationTime)
                .FirstOrDefaultAsync();

            if (otpVerification == null)
            {
                return "Invalid verification code";
            }

            if (otpVerification.ExpirationTime < DateTime.UtcNow)
            {
                return "Verification code has expired";
            }

            // Generate token for password reset
            string token = await userManager.GeneratePasswordResetTokenAsync(user);
            var result = await userManager.ResetPasswordAsync(user, token, resetPasswordDTO.Password);

            if (result.Succeeded)
            {
                return "Password Reset Successfully";
            }
            return result.Errors.ToList()[0].Description;
        }

        public async Task<bool> ActiveAccount(ActiveAccountDTO activeAccountDTO)
        {
            var user = await userManager.FindByEmailAsync(activeAccountDTO.Email);
            if (user == null)
            {
                return false;
            }

            // Verify OTP code
            var otpVerification = await context.OtpVerifications
                .Where(o => o.Email == activeAccountDTO.Email && o.OtpCode == activeAccountDTO.Code && !o.IsUsed)
                .OrderByDescending(o => o.ExpirationTime)
                .FirstOrDefaultAsync();

            if (otpVerification == null)
            {
                return false;
            }

            if (otpVerification.ExpirationTime < DateTime.UtcNow)
            {
                // Generate new OTP if expired
                string otpCode = GenerateCode.GenerateOtpCode(6);
                await StoreOtpForUser(user.Email, otpCode);
                await SendEmailWithOtp(user.Email, otpCode, "Please verify your email by entering the code below in the app");
                return false;
            }

            // Mark OTP as used
            otpVerification.IsUsed = true;
            await context.SaveChangesAsync();

            // Confirm email
            string token = await userManager.GenerateEmailConfirmationTokenAsync(user);
            var result = await userManager.ConfirmEmailAsync(user, token);

            return result.Succeeded;
        }

        public async Task StoreOtpForUser(string email, string otpCode, string purpose = "VerifyEmail")
        {
            DateTime expirationTime = DateTime.UtcNow.AddMinutes(10);

            var otpVerification = new OtpVerification
            {
                Email = email,
                OtpCode = otpCode,
                ExpirationTime = expirationTime,
                IsUsed = false,
                Purpose = purpose
            };

            context.OtpVerifications.Add(otpVerification);
            await context.SaveChangesAsync();
        }

        public async Task SendEmailWithOtp(string email, string otpCode, string message)
        {
            EmailDTO emailDTO = new EmailDTO
                (
                to: email,
                from: "elcentre.business@gmail.com",
                subject: "ElCentre Verification Code",
                content: EmailStringBody.send(email, otpCode, message)
                );

            await emailService.SendEmailAsync(emailDTO);
        }

        public async Task<bool> CheckOtpCode(string email, string otpCode)
        {
            // Verify OTP code
            var otpVerification = await context.OtpVerifications
                .Where(o => o.Email == email && o.OtpCode == otpCode && !o.IsUsed)
                .OrderByDescending(o => o.ExpirationTime)
                .FirstOrDefaultAsync();

            if (otpVerification == null)
            {
                return false;
            }

            if (otpVerification.ExpirationTime < DateTime.UtcNow)
            {
                return false;
            }

            otpVerification.IsUsed = true;
            await context.SaveChangesAsync();
            return true;

        }

        public async Task<bool> ResendOtpCode(string email)
        {
            var customer = await userManager.FindByEmailAsync(email);
            if (customer == null)
            {
                return false;
            }

            // Determine purpose based on user status
            string purpose = "VerifyEmail";

            // Check if there's an existing OTP for password reset
            var existingPasswordResetOtp = await context.OtpVerifications
                .Where(o => o.Email == email && o.Purpose == "ResetPassword" && !o.IsUsed)
                .OrderByDescending(o => o.ExpirationTime)
                .FirstOrDefaultAsync();

            if (existingPasswordResetOtp != null && existingPasswordResetOtp.ExpirationTime > DateTime.UtcNow)
            {
                // If there's a valid password reset OTP, continue with that purpose
                purpose = "ResetPassword";
            }
            else if (customer.EmailConfirmed)
            {
                // If email is already confirmed, this is likely a password reset
                purpose = "ResetPassword";
            }

            // Generate new OTP
            string otpCode = GenerateCode.GenerateOtpCode(6);

            // Store OTP in database
            await StoreOtpForUser(email, otpCode, purpose);

            // Send appropriate message based on purpose
            string message = purpose == "ResetPassword"
                ? "Please use the code below to reset your password"
                : "Please verify your email by entering the code below in the app";

            // Send OTP email
            await SendEmailWithOtp(email, otpCode, message);

            return true;
        }

        private bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }
    }
}
