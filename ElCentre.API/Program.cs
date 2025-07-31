using DotNetEnv;
using ElCentre.API.Middlewares;
using ElCentre.Contracts.Hubs;
using ElCentre.Core.Services;
using ElCentre.Infrastructure;
using ElCentre.Infrastructure.Repositories.Services;
using System.Reflection;
using Serilog;

namespace ElCentre.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Env.Load(); // Load environment variables from .env file
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("CORSPolicy", builder =>
                builder.AllowAnyMethod().AllowAnyHeader().AllowCredentials().SetIsOriginAllowed(_ => true));
            });
            builder.Services.AddSignalR();
            builder.Services.AddMemoryCache();
            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(s =>
            {
                s.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
                {
                    Title = "ElCentre.API",
                    Version = "v1"
                });
                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                s.IncludeXmlComments(xmlPath);
            });

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
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "ElCentre API V1");
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
            app.MapHub<NotificationsHub>("/hubs/notifications");

            app.MapControllers();

            app.Run();
        }
    }
}