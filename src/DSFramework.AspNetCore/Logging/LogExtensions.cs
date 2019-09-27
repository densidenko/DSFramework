using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace DSFramework.AspNetCore.Logging
{
    public static class LogExtensions
    {
        private static readonly IList _warnExTypes = new List<Type>
        {
            typeof(TaskCanceledException)
        };

        public static void LogUnobservedException(this ILogger logger, Exception e)
        {
            if (_warnExTypes.Contains(e.GetType()))
            {
                logger.LogWarning(e, "Unobserved Exception");
            }
            else
            {
                logger.LogError(e, "Unobserved Exception");
            }
        }
        public static void LogUnhandledException(this ILogger logger, UnhandledExceptionEventArgs e)
        {
            var ex = (Exception)e.ExceptionObject;
            if (e.IsTerminating)
            {
                logger.LogCritical(ex, "Unhandled Exception");
            }
            else
            {
                logger.LogError(ex, "Unhandled Exception");
            }
        }
    }
}