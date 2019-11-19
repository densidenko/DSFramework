using DSFramework.Web.AspNetCore.Middleware;
using Microsoft.AspNetCore.Builder;

namespace DSFramework.Web.AspNetCore.Extensions
{
    public static class ApplicationBuilderExtensions
    {
        public static void UseDefaultMvc(this IApplicationBuilder app)
        {
            app.UseExceptionHandler();
            app.UseMvc();
        }

        public static IApplicationBuilder UseRequestLogging(this IApplicationBuilder app) => app.UseMiddleware<RequestLoggingMiddleware>();
    }
}