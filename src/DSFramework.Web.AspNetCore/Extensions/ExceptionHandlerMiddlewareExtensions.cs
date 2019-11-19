using DSFramework.Web.AspNetCore.Middleware;
using Microsoft.AspNetCore.Builder;

namespace DSFramework.Web.AspNetCore.Extensions
{
    public static class ExceptionHandlerMiddlewareExtensions
    {
        public static IApplicationBuilder UseExceptionHandler(this IApplicationBuilder app) => app.UseMiddleware<ExceptionHandlerMiddleware>();
    }
}