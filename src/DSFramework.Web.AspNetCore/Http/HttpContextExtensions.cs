using System;
using System.Diagnostics;
using System.Globalization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Net.Http.Headers;

namespace DSFramework.Web.AspNetCore.Http
{
    /// <summary>
    ///     <see cref="HttpContext" /> extension methods.
    /// </summary>
    public static class HttpContextExtensions
    {
        private const string NO_CACHE = "no-cache";
        private const string NO_CACHE_MAX_AGE = "no-cache,max-age=";
        private const string NO_STORE = "no-store";
        private const string NO_STORE_NO_CACHE = "no-store,no-cache";
        private const string PUBLIC_MAX_AGE = "public,max-age=";
        private const string PRIVATE_MAX_AGE = "private,max-age=";

        /// <summary>
        ///     Adds the Cache-Control and Pragma HTTP headers by applying the specified cache profile to the HTTP context.
        /// </summary>
        /// <param name="context">The HTTP context.</param>
        /// <param name="cacheProfile">The cache profile.</param>
        /// <returns>The same HTTP context.</returns>
        /// <exception cref="System.ArgumentNullException">context or cacheProfile.</exception>
        public static HttpContext ApplyCacheProfile(this HttpContext context, CacheProfile cacheProfile)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (cacheProfile == null)
            {
                throw new ArgumentNullException(nameof(cacheProfile));
            }

            var headers = context.Response.Headers;

            if (!string.IsNullOrEmpty(cacheProfile.VaryByHeader))
            {
                headers[HeaderNames.Vary] = cacheProfile.VaryByHeader;
            }

            if (cacheProfile.NoStore == true)
            {
                // Cache-control: no-store, no-cache is valid.
                if (cacheProfile.Location == ResponseCacheLocation.None)
                {
                    headers[HeaderNames.CacheControl] = NO_STORE_NO_CACHE;
                    headers[HeaderNames.Pragma] = NO_CACHE;
                }
                else
                {
                    headers[HeaderNames.CacheControl] = NO_STORE;
                }
            }
            else
            {
                string cacheControlValue = null;
                var duration = cacheProfile.Duration.GetValueOrDefault().ToString(CultureInfo.InvariantCulture);
                switch (cacheProfile.Location)
                {
                    case ResponseCacheLocation.Any:
                        cacheControlValue = PUBLIC_MAX_AGE + duration;
                        break;
                    case ResponseCacheLocation.Client:
                        cacheControlValue = PRIVATE_MAX_AGE + duration;
                        break;
                    case ResponseCacheLocation.None:
                        cacheControlValue = NO_CACHE_MAX_AGE + duration;
                        headers[HeaderNames.Pragma] = NO_CACHE;
                        break;
                    default:
                        var exception = new NotImplementedException($"Unknown {nameof(ResponseCacheLocation)}: {cacheProfile.Location}");
                        Debug.Fail(exception.ToString());
                        throw exception;
                }

                headers[HeaderNames.CacheControl] = cacheControlValue;
            }

            return context;
        }

        /// <summary>
        ///     Gets an <see cref="IUrlHelper" /> instance. Uses <see cref="IUrlHelperFactory" /> and
        ///     <see cref="IActionContextAccessor" />.
        /// </summary>
        /// <param name="httpContext">The HTTP context.</param>
        /// <returns>An <see cref="IUrlHelper" /> instance for the current request.</returns>
        public static IUrlHelper GetUrlHelper(this HttpContext httpContext)
        {
            var services = httpContext.RequestServices;
            var actionContext = services.GetRequiredService<IActionContextAccessor>().ActionContext;
            var urlHelper = services.GetRequiredService<IUrlHelperFactory>().GetUrlHelper(actionContext);
            return urlHelper;
        }
    }
}