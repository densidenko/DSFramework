namespace DSFramework.AspNetCore.Prometheus.Helpers
{
    public class HttpHelper
    {
        public static bool IsFailed(int statusCode)
        {
            return statusCode >= 400 && statusCode <= 599;
        }
    }
}