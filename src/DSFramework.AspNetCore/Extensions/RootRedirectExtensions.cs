using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;

namespace DSFramework.AspNetCore.Extensions
{
    public static class RootRedirectExtensions
    {
        public static IApplicationBuilder UseRootRedirect(this IApplicationBuilder builder, string path)
        {
            return builder.MapWhen(httpContext => httpContext.Request.Path.Value == "/",
                                   appBuilder => appBuilder.Run(httpContext =>
                                   {
                                       httpContext.Response.Redirect(path);
                                       return Task.CompletedTask;
                                   }));
        }
    }
}