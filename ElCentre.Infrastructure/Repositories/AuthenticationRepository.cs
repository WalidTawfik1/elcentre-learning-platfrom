using ElCentre.Core.DTO;
using ElCentre.Core.Entities;
using ElCentre.Core.Interfaces;
using ElCentre.Core.Services;
using ElCentre.Core.Sharing;
using Microsoft.AspNetCore.Identity;
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


        public AuthenticationRepository(UserManager<AppUser> userManager, IEmailService emailService, SignInManager<AppUser> signInManager, IGenerateToken generateToken)
        {
            this.userManager = userManager;
            this.emailService = emailService;
            this.signInManager = signInManager;
            this.generateToken = generateToken;
        }

        public async Task<bool> ActiveAccount(ActiveAccountDTO activeAccountDTO)
        {
            var user = await userManager.FindByEmailAsync(activeAccountDTO.Email);
            if (user == null)
            {
                return false;
            }
            var result = await userManager.ConfirmEmailAsync(user, activeAccountDTO.Token);
            if (result.Succeeded)
            {
                return true;
            }
            var token = await userManager.GenerateEmailConfirmationTokenAsync(user);
            await SendEmail(user.Email, token, "Active", "Email Activation", "Please Active your email, click on button to active");
            return false;
        }

        public async Task<string> LoginAsync(LoginDTO loginDTO)
        {
            if (loginDTO == null) return null;
            var user = await userManager.FindByEmailAsync(loginDTO.Email);
            if (!user.EmailConfirmed)
            {
                string token = await userManager.GenerateEmailConfirmationTokenAsync(user);
                await SendEmail(email: user.Email, token, "Active", "Email Activation", "Please Active your email, click on button to active");
                return "Please Active your email first, we have sent the acivation link to your email";
            }
            var result = await signInManager.CheckPasswordSignInAsync(user, loginDTO.Password, lockoutOnFailure: true);
            if (result.Succeeded)
            {
                return generateToken.GetAndCreateToken(user);
            }
            return "Invalid Email or Password";
        }

        public async Task<string> RegisterAsync(RegisterDTO registerDTO)
        {
            if (registerDTO == null) return null;

            if (await userManager.FindByEmailAsync(registerDTO.Email) != null)
            {
                return "This email already exists";
            }
            var user = new AppUser
            {
                FirstName = registerDTO.FirstName,
                LastName = registerDTO.LastName,
                Email = registerDTO.Email,
                PhoneNumber = registerDTO.PhoneNumber,
                UserName = registerDTO.Email.Split('@')[0],
                Gender = registerDTO.Gender,
                DateOfBirth = registerDTO.DateOfBirth,
                UserType = registerDTO.UserType
            };
            var result = await userManager.CreateAsync(user, registerDTO.Password);
            if (!result.Succeeded)
            {
                return string.Join(", ", result.Errors.Select(e => e.Description));
            }
            // Add user to the specified role
            if (registerDTO.UserType == UserType.Instructor)
            {
                await userManager.AddToRoleAsync(user, "Instructor");
            }
            if (registerDTO.UserType == UserType.Student)
            {
                await userManager.AddToRoleAsync(user, "Student");
            }
            if (registerDTO.UserType == UserType.Admin)
            {
                await userManager.AddToRoleAsync(user, "Admin");
            }

            // Send email for account activation
            var token = await userManager.GenerateEmailConfirmationTokenAsync(user);
            await SendEmail(email: user.Email, token, "Active", "Email Activation", "Please Active your email, click on button to active");
            return "User Created Successfully";

        }

        public async Task SendEmail(string email, string code, string component, string subject, string message)
        {
            var result = new EmailDTO(
                email,
                "elcentre.business@gmail.com",
                subject,
                EmailStringBody.send(email, code, component, message));
            await emailService.SendEmailAsync(result);
        }

        public async Task<string> ResetPassword(ResetPasswordDTO resetPasswordDTO)
        {
            var user = await userManager.FindByEmailAsync(resetPasswordDTO.Email);
            if (user == null) return "Invalid Email";
            var result = await userManager.ResetPasswordAsync(user, resetPasswordDTO.Token, resetPasswordDTO.Password);
            if (result.Succeeded)
            {
                return "Password reset successfully";
            }
            return result.Errors.ToList()[0].Description;
        }

        public async Task<bool> SendEmailForgetPassword(string email)
        {
            var user = await userManager.FindByEmailAsync(email);
            if (user == null)
            {
                return false;
            }
            var token = await userManager.GeneratePasswordResetTokenAsync(user);
            await SendEmail(user.Email, token, "ResetPassword", "Reset Password", "Please Reset your password, click on button to reset");
            return true;
        }
    }
}
