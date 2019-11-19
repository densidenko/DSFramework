using System;

namespace DSFramework.Web.Metrics.Prometheus.Observers
{
    internal interface IControllerObserver
    {
        void Observe(string controller,
                     string methodName,
                     string application,
                     string clientEnv,
                     TimeSpan duration);

        void ReportEndpointUsage(string controller,
                                 string methodName,
                                 string application,
                                 string clientEnv);
    }
}