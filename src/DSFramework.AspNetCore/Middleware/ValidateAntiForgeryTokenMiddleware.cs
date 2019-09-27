using System.Threading.Tasks;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Http;

namespace DSFramework.AspNetCore.Middleware
{
    public class ValidateAntiForgeryTokenMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IAntiforgery _antiForgery;

        public ValidateAntiForgeryTokenMiddleware(RequestDelegate next, IAntiforgery antiForgery)
        {
            _next = next;
            _antiForgery = antiForgery;
        }

        public async Task Invoke(HttpContext context)
        {
            if (HttpMethods.IsPost(context.Request.Method))
            {
                await _antiForgery.ValidateRequestAsync(context);
            }

            await _next(context);
        }
    }
}