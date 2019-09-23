using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;

namespace DS.Logging.Timing
{
    public class TimeGuard : IDisposable
    {
        private readonly string _caller;
        private readonly ILogger _logger;
        private readonly Stopwatch _sw;
        private readonly TimeSpan _threshold;

        public TimeGuard(ILogger logger, TimeSpan threshold, string caller)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _caller = caller;
            _threshold = threshold;
            _sw = Stopwatch.StartNew();
        }

        public void Dispose()
        {
            _sw.Stop();
            if (_sw.Elapsed > _threshold)
            {
                _logger.LogWarning($"{_caller} takes more than {_threshold}. Elapsed={_sw.Elapsed}");
            }
        }

        public static TimeGuard Create(ILogger logger, TimeSpan timeSpan, [CallerMemberName] string caller = null)
        {
            return new TimeGuard(logger, timeSpan, caller);
        }

        public static TimeGuard Create(ILogger logger, [CallerMemberName] string caller = null)
        {
            return new TimeGuard(logger, TimeSpan.FromSeconds(5), caller);
        }
    }
}