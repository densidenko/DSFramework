namespace DSFramework.AspNetCore.Prometheus.Extensions
{
    public static class MetricExtensions
    {
        public static string AsMetricLabelValue<T>(this T val) where T : struct
        {
            return val.ToString().ToLower();
        }
    }
}