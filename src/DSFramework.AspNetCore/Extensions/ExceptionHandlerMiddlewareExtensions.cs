using DSFramework.AspNetCore.Middleware;
using Microsoft.AspNetCore.Builder;

namespace DSFramework.AspNetCore.Extensions
{
    public static class ExceptionHandlerMiddlewareExtensions
    {
        public static IApplicationBuilder UseExceptionHandler(this IApplicationBuilder app)
        {
            return app.UseMiddleware<ExceptionHandlerMiddleware>();
        }
    }
}