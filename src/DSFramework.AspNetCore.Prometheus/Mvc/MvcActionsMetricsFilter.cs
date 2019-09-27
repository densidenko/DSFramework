using System;
using System.Diagnostics;
using System.Net;
using DSFramework.AspNetCore.Prometheus.Helpers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using Prometheus.Client;

namespace DSFramework.AspNetCore.Prometheus.Mvc
{
    public class MvcActionsMetricsFilter : Attribute, IResourceFilter
    {
        private static readonly Counter _requestsCounter = global::Prometheus.Client.Metrics.CreateCounter(
            "mvc_requests_total",
            "MVC requests count",
            "code",
            "method",
            "controller",
            "action",
            "remote_ip",
            "route");

        private static readonly Counter _failedRequestsCounter = global::Prometheus.Client.Metrics.CreateCounter(
            "mvc_requests_failed_total",
            "failed MVC requests count",
            "code",
            "method",
            "controller",
            "action",
            "remote_ip",
            "route");

        private static readonly Histogram _durations = global::Prometheus.Client.Metrics.CreateHistogram(
            "mvc_requests_duration_seconds",
            "MVC requests durations",
            new[] { 0.0001, 0.0005, 0.001, 0.005, 0.01, 0.05, 0.1, 0.3, 0.6, 1, 2, 3, 5, 10, 15, 30, 60, 120, 180, 300 },
            "code",
            "method",
            "controller",
            "action",
            "remote_ip",
            "route");

        private Stopwatch _sw;
        private readonly ILogger<MvcActionsMetricsFilter> _logger;

        public MvcActionsMetricsFilter(ILogger<MvcActionsMetricsFilter> logger)
        {
            _logger = logger;
        }

        public void OnResourceExecuting(ResourceExecutingContext context)
        {
            _sw = Stopwatch.StartNew();
        }

        public void OnResourceExecuted(ResourceExecutedContext context)
        {
            try
            {
                var httpContext = context.HttpContext;
                if (httpContext == null)
                {
                    return;
                }

                var remoteIp = httpContext.Connection?.RemoteIpAddress?.ToString() ?? string.Empty;
                var route = ExtractRoute(context);
                var controller = context.RouteData.Values["controller"]?.ToString() ?? string.Empty;
                var action = context.RouteData.Values["action"]?.ToString() ?? string.Empty;
                var method = httpContext.Request.Method.ToLower();
                var code = context.Exception != null ? (int)HttpStatusCode.InternalServerError : httpContext.Response.StatusCode;
                _requestsCounter.Labels($"{code}", method, controller, action, remoteIp, route).Inc();

                _durations.Labels($"{code}", method, controller, action, remoteIp, route).Observe(_sw.Elapsed.TotalSeconds);
                if (HttpHelper.IsFailed(code))
                {
                    _failedRequestsCounter.Labels($"{code}", method, controller, action, remoteIp, route).Inc();
                }
            }
            catch (Exception exc)
            {
                _logger?.LogError($"{exc.Message}\n{exc.StackTrace}");
            }
        }

        private static string ExtractRoute(ResourceExecutedContext context)
        {
            var route = context.ActionDescriptor.AttributeRouteInfo.Template;
            var version = context.RouteData.Values["version"]?.ToString();
            if (!string.IsNullOrEmpty(version))
            {
                route = route.Replace("{version:apiVersion}", version);
            }
            return route;
        }
    }
}