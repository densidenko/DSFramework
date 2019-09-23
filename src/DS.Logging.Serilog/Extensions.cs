using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;

namespace DS.Logging.Serilog
{
    public static class Extensions
    {
        public static IWebHostBuilder UseSerilog(this IWebHostBuilder hostBuilder)
        {
            return hostBuilder.ConfigureLogging((ctx, logBuilder) => logBuilder
                                                                     .SetMinimumLevel(LogLevel.Debug)
                                                                     .ClearProviders()
                                                                     .AddSerilog(new LoggerConfiguration()
                                                                                 .ReadFrom.Configuration(ctx.Configuration)
                                                                                 .CreateLogger(),
                                                                                 true));
        }
    }
}