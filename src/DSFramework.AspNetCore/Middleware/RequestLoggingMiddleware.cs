using System.Threading.Tasks;
using DSFramework.AspNetCore.Http;
using Microsoft.AspNetCore.Http;
using Serilog.Context;

namespace DSFramework.AspNetCore.Middleware
{
    public class RequestLoggingMiddleware : IMiddleware
    {
        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            using (LogContext.PushProperty("RequestClientIpAddress", context.GetIp()))
            {
                using (LogContext.PushProperty("RequestUser", context.GetUser()))
                {
                    await next(context);
                }
            }
        }
    }
}