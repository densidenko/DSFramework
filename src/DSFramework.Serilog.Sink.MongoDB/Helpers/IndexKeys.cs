namespace DSFramework.Serilog.Sink.MongoDB.Helpers
{
    public class IndexKeys
    {
        public const string EVENT_ID = "EventId";
        public const string MESSAGE_TEMPLATE = "MessageTemplate";
        public const string LEVEL = "Level";
        public const string USER = "RequestUser";
        public const string APPLICATION = "App";
        public const string PRODUCT = "Product";
        public const string PROPERTIES = "Properties";
        public const string IP_ADDRESS = "RequestClientIpAddress";
        public const string TIMESTAMP_UTC = "TimestampUtc";
        public const string DATE = "Date";
        public const string REQUEST_ID = "RequestId";
        public const string CONNECTION_ID = "ConnectionId";
        public const string CORRELATION_ID = "CorrelationId";
    }
}