using AutoMapper;
using ElCentre.API.Helper;
using ElCentre.Core.DTO;
using ElCentre.Core.Entities;
using ElCentre.Core.Interfaces;
using ElCentre.Core.Services;
using ElCentre.Infrastructure.Data;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ElCentre.API.Controllers
{
    public class AccountController : BaseController
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly IConfiguration _configuration;
        private readonly IGenerateToken _generateToken;
        private readonly ElCentreDbContext _context;
        public AccountController(IUnitofWork work, IMapper mapper, UserManager<AppUser> userManager, SignInManager<AppUser> signInManager, IConfiguration configuration, IGenerateToken generateToken, ElCentreDbContext context) : base(work, mapper)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _configuration = configuration;
            _generateToken = generateToken;
            _context = context;
        }

        /// <summary>
        /// Registers a new user.
        /// </summary>
        /// <param name="registerDTO">The registration details.</param>
        /// <returns>A response indicating the result of the registration.</returns>
        [AllowAnonymous]
        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDTO registerDTO)
        {
            var result = await work.Authentication.RegisterAsync(registerDTO);
            if (result != "User Created Successfully")
            {
                return BadRequest(new APIResponse(400, result));
            }
            return Ok(new APIResponse(200, result));
        }

        /// <summary>
        /// Logs in a user.
        /// </summary>
        /// <param name="loginDTO">The login details.</param>
        /// <returns>A response indicating the result of the login.</returns>
        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDTO loginDTO)
        {
            var result = await work.Authentication.LoginAsync(loginDTO);
            if (result.StartsWith("Please") || result.StartsWith("Invalid"))
            {
                return BadRequest(new APIResponse(400, result));
            }
            Response.Cookies.Append("token", result, new CookieOptions
            {
                HttpOnly = true,
                SameSite = SameSiteMode.Lax,
                Secure = false,
                IsEssential = true,
                Expires = DateTime.Now.AddDays(7)
            });
            return Ok(new APIResponse(200, result));
        }

        /// <summary>
        /// Activates a user account.
        /// </summary>
        /// <param name="activeAccountDTO">The activation details.</param>
        /// <returns>A response indicating the result of the activation.</returns>
        [AllowAnonymous]
        [HttpPost("active-account")]
        public async Task<IActionResult> ActiveAccount(ActiveAccountDTO activeAccountDTO)
        {
            var result = await work.Authentication.ActiveAccount(activeAccountDTO);
            return result ? Ok(new APIResponse(200, "Account Activated Successfully")) :
                BadRequest(new APIResponse(200, "Please activate your account"));
        }

        /// <summary>
        /// Sends an email for password reset.
        /// </summary>
        /// <param name="email">The email address.</param>
        /// <returns>A response indicating the result of the email sending.</returns>
        [AllowAnonymous]
        [HttpGet("send-email-forget-password")]
        public async Task<IActionResult> SendEmailForgetPassword(string email)
        {
            var result = await work.Authentication.SendEmailForgetPassword(email);
            return result ? Ok(new APIResponse(200, "Email Sent Successfully")) :
                BadRequest(new APIResponse(200, "Email Not Sent"));
        }

        /// <summary>
        /// Resets the user password.
        /// </summary>
        /// <param name="resetPasswordDTO">The password reset details.</param>
        /// <returns>A response indicating the result of the password reset.</returns>
        [AllowAnonymous]
        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword(ResetPasswordDTO resetPasswordDTO)
        {
            var result = await work.Authentication.ResetPassword(resetPasswordDTO);
            if (result != "Password Reset Successfully")
            {
                return BadRequest(new APIResponse(400, result));
            }
            return Ok(new APIResponse(200, result));
        }

        /// <summary>
        /// Logs out a user.
        /// </summary>
        /// <returns></returns>
        [Authorize]
        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            Response.Cookies.Delete("token");
            return Ok(new APIResponse(200, "Logged out Successfully"));
        }

        /// <summary>
        /// Gets the profile of the authenticated user.
        /// </summary>
        /// <returns>User profile</returns>
        [Authorize]
        [HttpGet("profile")]
        public async Task<IActionResult> GetProfile()
        {
            try
            {
                // The repository method will get the ID from the authenticated user
                var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized("User not authenticated, Please login or register.");
                }

                var user = await work.UserRepository.GetUserProfileAsync(userId);
                if (user == null)
                {
                    return NotFound($"User profile not found.");
                }

                return Ok(user);
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Gets all instructors.
        /// </summary>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpGet("get-all-instructors")]
        public async Task<IActionResult> GetAllInstructors()
        {
            try
            {
                var instructors = await work.UserRepository.GetAllInstructorsAsync();
                if (instructors == null || !instructors.Any())
                {
                    return NotFound("No instructors found.");
                }
                return Ok(instructors);
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Gets an instructor by their ID.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpGet("get-instructor-by-id/{id}")]
        public async Task<IActionResult> GetInstructorById(string id)
        {
            try
            {
                var instructor = await work.UserRepository.GetInstructorById(id);
                if (instructor == null)
                {
                    return NotFound($"Instructor with ID {id} not found.");
                }
                return Ok(instructor);
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Updates the profile of the authenticated user.
        /// </summary>
        /// <param name="customerDTO"></param>
        /// <returns></returns>
        [Authorize]
        [HttpPut("edit-profile")]
        public async Task<IActionResult> EditProfile([FromForm] UpdateUserDTO userDTO)
        {
            try
            {
                var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized("User not authenticated, Please login or register.");
                }

                var updatedUser = await work.UserRepository.UpdateUserProfileAsync(userId, userDTO);
                return Ok(updatedUser);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Verifies the OTP code sent to the user's email.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("verify-otp")]
        public async Task<IActionResult> VerifyOtp([FromBody] OTPVerify request)
        {
            bool isValid = await work.Authentication.CheckOtpCode(request.Email, request.Code);

            if (isValid)
            {
                return Ok(new { success = true, message = "Verification code is valid" });
            }

            return BadRequest(new { success = false, message = "Invalid or expired verification code" });
        }

        /// <summary>
        /// Resends the OTP code to the user's email.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("resend-otp")]
        public async Task<IActionResult> ResendOtp([FromBody] OTPResend request)
        {
            bool result = await work.Authentication.ResendOtpCode(request.Email);

            if (result)
            {
                return Ok(new { success = true, message = "Verification code sent successfully" });
            }

            return BadRequest(new { success = false, message = "Failed to send verification code" });
        }

        /// <summary>
        /// Initiates Google login with role selection.
        /// </summary>
        /// <param name="role">The role to assign to the user (Student or Instructor).</param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpGet("google-login")]
        public IActionResult GoogleLogin(string? role = "Student")
        {
            // Validate role
            if (role != "Student" && role != "Instructor" && role != null)
            {
                return BadRequest(new APIResponse(400, "Invalid role. Role must be either 'Student' or 'Instructor'."));
            }

            // Check if Google authentication is configured
            var googleClientId = _configuration["Authentication:Google:ClientId"];
            var googleClientSecret = _configuration["Authentication:Google:ClientSecret"];
            
            if (string.IsNullOrEmpty(googleClientId) || string.IsNullOrEmpty(googleClientSecret))
            {
                return BadRequest(new APIResponse(400, "Google authentication is not configured. Please contact the administrator."));
            }

            var redirectUrl = Url.Action("GoogleResponse", "Account", new { role }, Request.Scheme);
            var properties = _signInManager.ConfigureExternalAuthenticationProperties("Google", redirectUrl);
            return Challenge(properties, "Google");
        }

        /// <summary>
        /// Handles the Google login response and assigns the selected role.
        /// </summary>
        /// <param name="role">The role to assign to the user.</param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpGet("google-response")]
        public async Task<IActionResult> GoogleResponse(string role = "Student")
        {
            try
            {
                // Validate role
                if (role != "Student" && role != "Instructor")
                {
                    role = "Student"; // Default to Student if invalid role
                }

                // Clear any existing external cookies
                await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);
                
                var info = await _signInManager.GetExternalLoginInfoAsync();
                if (info == null)
                {
                    return BadRequest(new APIResponse(400, "Error loading external login information. Please try logging in again."));
                }

                // Look for the user in your application
                var user = await _userManager.FindByLoginAsync(info.LoginProvider, info.ProviderKey);
                bool isNewUser = false;

                if (user == null)
                {
                    var email = info.Principal.FindFirstValue(ClaimTypes.Email);
                    if (string.IsNullOrEmpty(email))
                    {
                        return BadRequest(new APIResponse(400, "Google account did not provide an email."));
                    }

                    // Check if user already exists by email
                    user = await _userManager.FindByEmailAsync(email);
                    if (user == null)
                    {
                        isNewUser = true;
                        user = new AppUser
                        {
                            UserName = info.Principal.Identity?.Name?.Replace(" ", "") ?? email,
                            FirstName = info.Principal.FindFirstValue(ClaimTypes.GivenName) ?? "Unknown",
                            LastName = info.Principal.FindFirstValue(ClaimTypes.Surname) ?? "User",
                            Email = email,
                            EmailConfirmed = true,
                            UserType = role,
                            CreatedAt = DateTime.Now,
                            Gender = info.Principal.FindFirstValue(ClaimTypes.Gender) ?? "Unknown",
                            DateOfBirth = DateOnly.TryParse(info.Principal.FindFirstValue(ClaimTypes.DateOfBirth), out var parsedDate)
                                ? parsedDate : DateOnly.FromDateTime(DateTime.Now),
                            PhoneNumber = info.Principal.FindFirstValue(ClaimTypes.MobilePhone) ?? "0123456789"                           
                        };

                        var result = await _userManager.CreateAsync(user);
                        if (!result.Succeeded)
                        {
                            return BadRequest(new APIResponse(400, "User creation failed."));
                        }
                        
                        // Add user to role
                        var roleResult = await _userManager.AddToRoleAsync(user, role);
                        if (!roleResult.Succeeded)
                        {
                            return BadRequest(new APIResponse(400, $"Failed to assign {role} role."));
                        }
                    }

                    // Link Google login with user
                    var loginResult = await _userManager.AddLoginAsync(user, info);
                    if (!loginResult.Succeeded)
                    {
                        return BadRequest(new APIResponse(400, "Failed to associate Google login with user."));
                    }
                }

                // Sign in the user using cookie authentication
                await _signInManager.SignInAsync(user, isPersistent: false);

                // Generate JWT token
                var token = _generateToken.GetAndCreateToken(user);

                // Set token in cookie
                Response.Cookies.Append("token", token, new CookieOptions
                {
                    HttpOnly = true,
                    SameSite = SameSiteMode.Lax,
                    Secure = false,
                    IsEssential = true,
                    Expires = DateTime.Now.AddDays(7)
                });

                // Return to frontend with token and user status
                return Redirect($"http://localhost:8080/google-bridge?token={token}&isNewUser={isNewUser}&role={user.UserType}");
            }
            catch (Exception ex)
            {
                return StatusCode(500, new APIResponse(500, $"Internal server error: {ex.Message}"));
            }
        }
    }
}
