using System.Reflection;
using System.Security.Claims;
using System.Security.Principal;
using DSFramework.Runtime.Session;
using DSFramework.Security.Authorization;
using DSFramework.Web.AspNetCore.Authorization;
using DSFramework.Web.AspNetCore.Runtime;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace DSFramework.Web.AspNetCore.Extensions
{
    public static class AspNetExtensions
    {
        public static IMvcBuilder AddDefaultMvc<T>(this IServiceCollection services)
        {
            services.AddOptions();

            var builder = services.AddMvc(options =>
                                  {
#if DEBUG
                                      options.Filters.Add(new AllowAnonymousFilter());
#endif
                                      options.OutputFormatters.RemoveType<StringOutputFormatter>();
                                      options.OutputFormatters.Add(new StringOutputFormatter());
                                  })
                                  .AddJsonOptions(options =>
                                  {
                                      options.AllowInputFormatterExceptionMessages = true;
                                      options.SerializerSettings.TypeNameHandling = TypeNameHandling.Auto;
                                      options.SerializerSettings.ContractResolver = new DefaultContractResolver();
                                  });

            var assembly = typeof(T).GetTypeInfo().Assembly;

            return builder;
        }

        public static IServiceCollection AddCommonWeb(this IServiceCollection services)
        {
            services.AddHttpContextAccessor();
            services.AddScoped<IUserSession, UserSession>();
            services.AddTransient<PermissionDependencyContext>();
            services.AddSingleton<IPermissionService, PermissionService>();
            services.AddScoped<IPrincipal>(provider => provider.GetService<IHttpContextAccessor>()?.HttpContext?.User ?? ClaimsPrincipal.Current);
            services.AddSingleton<IAuthorizationPolicyProvider, AuthorizationPolicyProvider>();
            services.AddSingleton<IAuthorizationHandler, PermissionAuthorizationRequirement>();

            return services;
        }
    }
}