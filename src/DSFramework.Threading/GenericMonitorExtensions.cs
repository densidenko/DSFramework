using System;

namespace DSFramework.Threading
{
    public static class GenericMonitorExtensions
    {
        private sealed class Token<T> : IDisposable
        {
            private readonly T _key;
            private readonly bool _lockTaken;
            private GenericMonitor<T> _sync;

            public Token(GenericMonitor<T> sync, T key)
            {
                _sync = sync;
                _key = key;
                _lockTaken = sync.Enter(key);
            }

            public void Dispose()
            {
                if (_sync == null)
                {
                    return;
                }

                _sync.Exit(_key, _lockTaken);
                _sync = null;
            }
        }

        public static IDisposable Lock<T>(this GenericMonitor<T> obj, T key) => new Token<T>(obj, key);
    }
}