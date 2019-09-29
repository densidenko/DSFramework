using System.Threading.Tasks;
using DSFramework.AspNetCore.Http;
using Microsoft.AspNetCore.Http;
using Serilog.Context;

namespace DSFramework.AspNetCore.Middleware
{
    public class RequestLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        public RequestLoggingMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            using (LogContext.PushProperty("RequestClientIpAddress", context.GetIp()))
            {
                using (LogContext.PushProperty("RequestUser", context.GetUser()))
                {
                    await _next(context);
                }
            }
        }
    }
}