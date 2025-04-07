using ElCentre.API.Helper;
using Microsoft.Extensions.Caching.Memory;
using System.Net;
using System.Text.Json;

namespace ElCentre.API.Middlewares
{
    public class ExceptionsMiddleware
    {
        private readonly RequestDelegate next;
        private readonly IHostEnvironment env;
        private readonly IMemoryCache memoryCache;
        private readonly TimeSpan rateLimitWindow = TimeSpan.FromSeconds(30);

        public ExceptionsMiddleware(RequestDelegate next, IHostEnvironment env, IMemoryCache memoryCache)
        {
            this.next = next;
            this.env = env;
            this.memoryCache = memoryCache;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                ApplySecurity(context);

                if (!IsRequestAllowed(context))
                {
                    context.Response.StatusCode = (int)HttpStatusCode.TooManyRequests;
                    context.Response.ContentType = "application/json";
                    var response = new APIExceptions(context.Response.StatusCode, "Too many requests, Try again later");
                    var json = JsonSerializer.Serialize(response);
                    await context.Response.WriteAsync(json);
                    return;
                }
                await next(context);
            }
            catch (Exception ex)
            {
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                context.Response.ContentType = "application/json";
                var response = env.IsDevelopment() ?
                    new APIExceptions(context.Response.StatusCode, ex.Message, ex.StackTrace)
                    : new APIExceptions(context.Response.StatusCode, ex.Message);
                var json = JsonSerializer.Serialize(response);
                await context.Response.WriteAsync(json);
                var innerException = ex.InnerException?.Message;

            }
        }

        private bool IsRequestAllowed(HttpContext context)
        {
            var ip = context.Connection.RemoteIpAddress.ToString();
            var cashKey = $"Rate:{ip}";
            var dateNow = DateTime.Now;

            var (timeStamp, count) = memoryCache.GetOrCreate(cashKey, entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = rateLimitWindow;
                return (dateNow, 0);
            });
            if (dateNow - timeStamp < rateLimitWindow)
            {
                if (count >= 10)
                {
                    return false;
                }
                memoryCache.Set(cashKey, (timeStamp, count += 1), rateLimitWindow);
            }
            else
            {
                memoryCache.Set(cashKey, (timeStamp, count), rateLimitWindow);
            }
            return true;

        }

        private void ApplySecurity(HttpContext context)
        {
            context.Response.Headers.Add("X-Content-Type-Options", "nosniff");
            context.Response.Headers.Add("X-Frame-Options", "DENY");
            context.Response.Headers.Add("X-XSS-Protection", "1; mode=block");
            context.Response.Headers.Add("Content-Security-Policy", "default-src 'self'");
        }

    }
}
