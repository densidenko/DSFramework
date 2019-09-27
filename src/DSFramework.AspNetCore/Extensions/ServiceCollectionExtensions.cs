using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http.Headers;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace DSFramework.AspNetCore.Extensions
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

        public static void UseDefaultMvc(this IApplicationBuilder app)
        {
            app.UseExceptionHandler();
            app.UseMvc();
        }
    }
}