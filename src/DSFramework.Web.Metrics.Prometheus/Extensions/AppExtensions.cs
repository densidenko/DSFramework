using System.Diagnostics;
using System.Net;
using System.Threading.Tasks;
using DSFramework.Web.Metrics.Prometheus.Helpers;
using DSFramework.Web.Metrics.Prometheus.Mvc;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Prometheus.Client;
using Prometheus.Client.Collectors;

namespace DSFramework.Web.Metrics.Prometheus.Extensions
{
    public static class AppExtensions
    {
        private static readonly Counter _counter =
            global::Prometheus.Client.Metrics.CreateCounter("metrics_calls_total", "Total amount of metrics calls");

        private static readonly Counter _failedRequestsCounter =
            global::Prometheus.Client.Metrics.CreateCounter("http_requests_failed_total",
                                                            "Failed http requests count",
                                                            "code",
                                                            "method",
                                                            "remote_ip");

        private static readonly Counter _requestsCounter =
            global::Prometheus.Client.Metrics.CreateCounter("http_requests_total", "http requests count", "code", "method", "remote_ip");

        private static readonly Histogram _durations = global::Prometheus.Client.Metrics.CreateHistogram("http_requests_duration_seconds",
                                                                                                         "Http request durations",
                                                                                                         new[]
                                                                                                         {
                                                                                                             0.0001, 0.0005, 0.001, 0.005, 0.01,
                                                                                                             0.05, 0.1, 0.3, 0.6, 1, 2, 3,
                                                                                                             5, 10, 15, 30, 60, 120, 180, 300
                                                                                                         },
                                                                                                         "code",
                                                                                                         "method",
                                                                                                         "remote_ip");

        public static IApplicationBuilder UsePrometheusMetrics(this IApplicationBuilder app, string path = "/metrics")
            => app.Map(path, builder => builder.Run(Handle));

        public static void UseHttpMetrics(this IApplicationBuilder app)
            => app.Use(async (context, next) =>
            {
                var method = context.Request.Method.ToLower();
                var stopWatch = Stopwatch.StartNew();
                var code = string.Empty;
                var remoteIp = context.Connection?.RemoteIpAddress?.ToString() ?? string.Empty;

                try
                {
                    await next.Invoke();
                    var response = context.Response;
                    code = response.StatusCode.ToString();
                    if (HttpHelper.IsFailed(response.StatusCode))
                    {
                        _failedRequestsCounter.Labels(code, method, remoteIp).Inc();
                    }
                }
                catch
                {
                    code = HttpStatusCode.InternalServerError.ToString("D");
                    _failedRequestsCounter.Labels(code, method, remoteIp).Inc();
                    throw;
                }
                finally
                {
                    _durations.Labels(code, method, remoteIp).Observe(stopWatch.Elapsed.TotalSeconds);
                    _requestsCounter.Labels(code, method, remoteIp).Inc();
                }
            });

        public static void AddMvcMetrics(this MvcOptions options) => options.Filters.Add(new TypeFilterAttribute(typeof(MvcActionsMetricsFilter)));

        private static Task Handle(HttpContext context)
        {
            var request = context.Request;
            var response = context.Response;

            if (request.Method == HttpMethods.Get)
            {
                _counter.Inc();

                var registry = CollectorRegistry.Instance;
                response.StatusCode = 200;
                ScrapeHandler.Process(registry, response.Body);
            }
            else
            {
                response.StatusCode = StatusCodes.Status405MethodNotAllowed;
            }

            return Task.CompletedTask;
        }
    }
}