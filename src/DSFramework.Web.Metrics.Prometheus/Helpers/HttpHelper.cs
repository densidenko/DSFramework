namespace DSFramework.Web.Metrics.Prometheus.Helpers
{
    public class HttpHelper
    {
        public static bool IsFailed(int statusCode) => statusCode >= 400 && statusCode <= 599;
    }
}