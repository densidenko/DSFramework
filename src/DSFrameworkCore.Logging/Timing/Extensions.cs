using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;

namespace DS.Logging.Timing
{
    [SuppressMessage("ReSharper", "ExplicitCallerInfoArgument")]
    public static class Extensions
    {
        public static void LogTime(
            this ILogger logger,
            Action action,
            string call,
            [CallerMemberName] string caller = null)
        {
            if (caller != null)
            {
                call = $"{caller}:{call}";
            }
            logger.LogDebug($"{call} started.");
            var stopwatch = Stopwatch.StartNew();
            try
            {
                action();
                logger.LogDebug($"{call} finished. Elapsed={stopwatch.Elapsed}");
            }
            catch (Exception e)
            {
                logger.LogDebug($"{call} failed. Elapsed={stopwatch.Elapsed}. Error={e.Message}");
                throw;
            }
        }

        public static TimeGuard CreateTimeGuard(this ILogger logger, [CallerMemberName] string caller = null)
        {
            if (logger == null)
            {
                throw new ArgumentNullException(nameof(logger));
            }

            return TimeGuard.Create(logger, caller);
        }

        public static TimeGuard CreateTimeGuard(
            this ILogger logger,
            TimeSpan timeSpan,
            [CallerMemberName] string caller = null)
        {
            if (logger == null)
            {
                throw new ArgumentNullException(nameof(logger));
            }

            return TimeGuard.Create(logger, timeSpan, caller);
        }
    }
}