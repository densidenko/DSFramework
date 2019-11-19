using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;

namespace DSFramework.Logging.Serilog
{
    public static class Extensions
    {
        public static IWebHostBuilder UseSerilog(this IWebHostBuilder hostBuilder)
            => hostBuilder.ConfigureLogging((ctx, logBuilder)
                                                => logBuilder.SetMinimumLevel(LogLevel.Debug)
                                                             .ClearProviders()
                                                             .AddSerilog(new LoggerConfiguration()
                                                                         .ReadFrom.Configuration(ctx.Configuration)
                                                                         .CreateLogger(),
                                                                         dispose: true));
    }
}