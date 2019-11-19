namespace DSFramework.Web.Metrics.Prometheus.Extensions
{
    public static class MetricExtensions
    {
        public static string AsMetricLabelValue<T>(this T val) where T : struct => val.ToString().ToLower();
    }
}