using System;

namespace DSFramework.AspNetCore.Prometheus.Observers
{
    internal interface IControllerObserver
    {
        void ReportEndpointUsage(string controller, string methodName, string application, string clientEnv);
        void Observe(string controller, string methodName, string application, string clientEnv, TimeSpan duration);
    }
}