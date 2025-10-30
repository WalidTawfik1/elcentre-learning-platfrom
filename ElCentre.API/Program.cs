using DotNetEnv;
using ElCentre.API.Middlewares;
using ElCentre.Contracts.Hubs;
using ElCentre.Core.Services;
using ElCentre.Infrastructure;
using ElCentre.Infrastructure.Repositories.Services;
using System.Reflection;
using Serilog;
using Asp.Versioning.Conventions;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.SwaggerGen;
using Asp.Versioning.ApiExplorer;
using Asp.Versioning;

namespace ElCentre.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Env.Load(); // Load environment variables from .env file
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddCors(options =>
            {
                options.AddPolicy("CORSPolicy", builder =>
                    builder
                        .WithOrigins("https://elcentre-learn.vercel.app", "http://localhost:8080")
                        .AllowAnyMethod()
                        .AllowAnyHeader()
                        .AllowCredentials()
                );
            });

            builder.Services.AddSignalR();
            builder.Services.AddMemoryCache();
            builder.Services.AddControllers();

            builder.Services.AddApiVersioning(options =>
            {
                options.AssumeDefaultVersionWhenUnspecified = true;
                options.DefaultApiVersion = new Asp.Versioning.ApiVersion(1, 0);
                options.ReportApiVersions = true;
                options.ApiVersionReader = new UrlSegmentApiVersionReader();
            }).AddMvc(options =>
            {
                options.Conventions.Add(new VersionByNamespaceConvention());
            }).AddApiExplorer(options =>
            {
                options.GroupNameFormat = "'v'V";
                options.SubstituteApiVersionInUrl = true;
            });

            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddTransient<IConfigureOptions<SwaggerGenOptions>, ConfigureSwaggerOptions>();
            builder.Services.AddSwaggerGen();

            // Register notification service
            builder.Services.AddScoped<INotificationService, NotificationService>();

            // Add Infrastructure services
            builder.Services.InfrastructureConfiguration(builder.Configuration);
            // Add AutoMapper
            builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

            Log.Logger = new LoggerConfiguration()
             .WriteTo.File("Logs/paymob-log.txt", rollingInterval: RollingInterval.Day)
             .CreateLogger();

            builder.Host.UseSerilog();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            // if (app.Environment.IsDevelopment())
            //{
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                var apiVersionDescriptionProvider = app.Services.GetRequiredService<IApiVersionDescriptionProvider>();
                
                foreach (var description in apiVersionDescriptionProvider.ApiVersionDescriptions)
                {
                    c.SwaggerEndpoint($"/swagger/{description.GroupName}/swagger.json", 
                        $"ElCentre API {description.GroupName.ToUpperInvariant()}");
                }
                
                c.RoutePrefix = string.Empty; // Set Swagger UI at the app's root
            });

            app.UseCors("CORSPolicy");

            app.UseMiddleware<ExceptionsMiddleware>();

            app.UseAuthentication();

            app.UseAuthorization();

            app.UseStaticFiles();

            app.UseStatusCodePagesWithReExecute("/errors/{0}");

            app.UseHttpsRedirection();

            // Map SignalR hub
            app.MapHub<NotificationsHub>("/hubs/notifications").RequireCors("CORSPolicy");

            app.MapControllers();

            app.Run();
        }
    }

    public class ConfigureSwaggerOptions : IConfigureOptions<SwaggerGenOptions>
    {
        private readonly IApiVersionDescriptionProvider _provider;

        public ConfigureSwaggerOptions(IApiVersionDescriptionProvider provider)
        {
            _provider = provider;
        }

        public void Configure(SwaggerGenOptions options)
        {
            foreach (var description in _provider.ApiVersionDescriptions)
            {
                options.SwaggerDoc(description.GroupName, CreateVersionInfo(description));
            }

            var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            options.IncludeXmlComments(xmlPath);
        }

        private static Microsoft.OpenApi.Models.OpenApiInfo CreateVersionInfo(ApiVersionDescription description)
        {
            var info = new Microsoft.OpenApi.Models.OpenApiInfo()
            {
                Title = "ElCentre.API",
                Version = description.ApiVersion.ToString()
            };

            if (description.IsDeprecated)
            {
                info.Description += " This API version has been deprecated.";
            }

            return info;
        }
    }
}