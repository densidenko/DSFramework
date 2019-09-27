using System;
using DSFramework.Extensions;
using Prometheus.Client;

namespace DSFramework.AspNetCore.Prometheus.Observers
{
    public class PrometheusControllerObserver<T> : IControllerObserver
    {
        private readonly Counter _total = Metrics.CreateCounter("controller_calls_total",
                                                                "Total of entity controller calls",
                                                                "version",
                                                                "obsolete",
                                                                "controller",
                                                                "methodName",
                                                                "entityType",
                                                                "application",
                                                                "clientEnv");

        private readonly Histogram _duration = Metrics.CreateHistogram("controller_calls_duration_seconds",
                                                                       "Duration of entity controller calls",
                                                                       new[]
                                                                       {
                                                                           0.0001, 0.0005, 0.001, 0.005, 0.01, 0.05, 0.1, 0.2, 0.3, 0.5, 1, 2, 3, 5,
                                                                           10, 30, 60, 180, 360
                                                                       },
                                                                       "version",
                                                                       "obsolete",
                                                                       "controller",
                                                                       "methodName",
                                                                       "entityType",
                                                                       "application",
                                                                       "clientEnv");

        private readonly string _controllerVersion;
        private readonly string _obsolete;
        private readonly string _entityType;

        public PrometheusControllerObserver(string controllerVersion, bool obsolete)
        {
            _controllerVersion = controllerVersion;
            _obsolete = obsolete ? "1" : "0";
            _entityType = typeof(T).ReadableName();
        }

        public void ReportEndpointUsage(string controller, string methodName, string application, string clientEnv)
        {
            _total.Labels(_controllerVersion, _obsolete, controller, methodName, _entityType, application, clientEnv).Inc();
        }

        public void Observe(
            string controller,
            string methodName,
            string application,
            string clientEnv,
            TimeSpan duration)
        {
            _duration.Labels(_controllerVersion, _obsolete, controller, methodName, _entityType, application, clientEnv)
                     .Observe(duration.TotalSeconds);
        }
    }
}