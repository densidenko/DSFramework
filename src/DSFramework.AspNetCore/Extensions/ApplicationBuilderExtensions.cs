using Microsoft.AspNetCore.Builder;

namespace DSFramework.AspNetCore.Extensions
{
    public static class ApplicationBuilderExtensions
    {
        public static void UseDefaultMvc(this IApplicationBuilder app)
        {
            app.UseExceptionHandler();
            app.UseMvc();
        }
    }
}