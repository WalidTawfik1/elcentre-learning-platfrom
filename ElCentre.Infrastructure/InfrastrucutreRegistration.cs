using ElCentre.Core.DTO;
using ElCentre.Core.Entities;
using ElCentre.Core.Interfaces;
using ElCentre.Core.Services;
using ElCentre.Infrastructure.Data;
using ElCentre.Infrastructure.Repositories;
using ElCentre.Infrastructure.Repositories.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;
using X.Paymob.CashIn;

namespace ElCentre.Infrastructure
{
    public static class InfrastrucutreRegistration
    {
        public static IServiceCollection InfrastructureConfiguration(this IServiceCollection services, IConfiguration configuration)
        {
            // Load environment variables from .env file
            DotNetEnv.Env.Load(); // Ensure DotNetEnv package is installed and properly referenced

            // Register IUnitofWork service
            services.AddScoped<IUnitofWork, UnitofWork>();

            // Register IEmailService
            services.AddScoped<IEmailService, EmailService>();

            // Register IGenerateToken service
            services.AddScoped<IGenerateToken, GenerateToken>();

            // Register ICourseThumbnail service
            services.AddSingleton<ICourseThumbnailService, CourseThumbnailService>();

            // Register the IFileProvider service
            services.AddSingleton<IFileProvider>(new PhysicalFileProvider(Path.Combine(Directory.GetCurrentDirectory())));

            // Register the ICloudinarySettings service
            services.Configure<CloudinarySettings>(configuration.GetSection("CloudinarySettings"));

            // Register the IVideoService
            services.AddScoped<IVideoService, VideoService>();

            // Register AddHttpContextAccessor to get user info
            services.AddHttpContextAccessor();

            // Register the Paymob service
            services.AddPaymobCashIn(options =>
            {
                options.ApiKey = configuration["Paymob:ApiKey"];
                options.Hmac = configuration["Paymob:HMAC"];
            });

            // Register the IPaymob service
            services.AddScoped<IPaymobService, PaymobService>();

            // Register AppDbContext with SQL Server
            services.AddDbContext<ElCentreDbContext>((options) =>
            {
                options.UseSqlServer(configuration.GetConnectionString("ElCentreDatabase"));
            });

            // Register Identity service
            services.AddIdentity<AppUser, IdentityRole>(options =>
            {
                options.User.RequireUniqueEmail = true;
                options.Password.RequireDigit = true;
                options.Password.RequiredLength = 6;
                options.Password.RequireNonAlphanumeric = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireLowercase = true;
            }).AddEntityFrameworkStores<ElCentreDbContext>()
              .AddDefaultTokenProviders();

            services.AddAuthentication(opt =>
            {
                opt.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                opt.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                opt.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            }).AddCookie(c =>
            {
                c.Cookie.Name = "token";
                c.Cookie.SecurePolicy = CookieSecurePolicy.None;
                c.Cookie.SameSite = SameSiteMode.Lax;
                c.Events.OnRedirectToLogin = context =>
                {
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    return Task.CompletedTask;
                };

            }).AddJwtBearer(opt =>
            {
                opt.RequireHttpsMetadata = false;
                opt.SaveToken = true;
                opt.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Token:Secret"])),
                    ValidateIssuer = true,
                    ValidIssuer = configuration["Token:Issuer"],
                    ValidateAudience = false,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };
                opt.Events = new JwtBearerEvents()
                {
                    OnMessageReceived = context =>
                    {
                        context.Token = context.Request.Cookies["token"];
                        return Task.CompletedTask;
                    }
                };
            }).AddGoogle(googleOptions =>
            {
               googleOptions.ClientId = configuration["Authentication:Google:ClientId"];
               googleOptions.ClientSecret = configuration["Authentication:Google:ClientSecret"];
               googleOptions.CallbackPath = "/signin-google";
               googleOptions.SaveTokens = true;
            });

            return services;
        }
    }
    
}
