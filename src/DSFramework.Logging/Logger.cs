using DSFramework.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace DSFramework.Logging
{
    public static class Logger
    {
        private static ILoggerFactory _factory;

        public static ILoggerFactory LoggerFactory => _factory ?? (_factory = new NullLoggerFactory());

        public static ILogger Create<T>()
        {
            var readableName = typeof(T).ReadableName();
            return LoggerFactory.CreateLogger(readableName);
        }

        public static void ConfigureLogger(ILoggerFactory factory)
        {
            _factory = factory;
        }
    }
}