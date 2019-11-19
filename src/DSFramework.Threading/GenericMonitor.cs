using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;

namespace DSFramework.Threading
{
    public class GenericMonitor<T>
    {
        /// <summary>
        ///     Describes locked objects
        /// </summary>
        private sealed class LockDescriptor
        {
            private int _waitingThreads;
            private int _workingThreads;

            public int WaitingThreads => _waitingThreads;

            /// <summary>
            ///     Number of entrance within current thread
            /// </summary>
            public int WorkingThreads => _workingThreads;

            public int StartWait() => Interlocked.Increment(ref _waitingThreads);

            public int StartWork() => Interlocked.Increment(ref _workingThreads);

            public int EndWait() => Interlocked.Decrement(ref _waitingThreads);

            public int EndWork() => Interlocked.Decrement(ref _workingThreads);
        }

        private sealed class MultiToken : IDisposable
        {
            private readonly List<Token> _tokens = new List<Token>();

            public MultiToken(GenericMonitor<T> sync, IEnumerable<T> keys)
            {
                foreach (var key in keys.Distinct().OrderBy(k => k))
                {
                    _tokens.Add(new Token(sync, key));
                }
            }

            public void Dispose()
            {
                foreach (var token in _tokens)
                {
                    token.Dispose();
                }

                _tokens.Clear();
            }
        }

        private sealed class Token : IDisposable
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

        private readonly Dictionary<T, LockDescriptor> _locks = new Dictionary<T, LockDescriptor>();

        public IDisposable Lock(T key) => new Token(this, key);

        public IDisposable Lock(IEnumerable<T> keys) => new MultiToken(this, keys);

        public bool Enter(T key)
        {
            var lockTaken = false;
            var newLock = false;

            var managedThreadId = Thread.CurrentThread.ManagedThreadId;

            Debug.WriteLine($"Enter. Key={key}, ManagedThreadId={managedThreadId}");

            LockDescriptor lockDescriptor;
            int waiters;
            lock (_locks)
            {
                Debug.WriteLine($"Enter. lock (_locks). Key={key}, ManagedThreadId={managedThreadId}");

                if (!_locks.TryGetValue(key, out lockDescriptor))
                {
                    Debug.WriteLine($"Enter. LockDescriptor created. Key={key}, ManagedThreadId={managedThreadId}");

                    lockDescriptor = new LockDescriptor();
                    _locks[key] = lockDescriptor;

                    Debug.WriteLine($"Enter. Start waiting for new lock. Key={key}, ManagedThreadId={managedThreadId}");

                    Monitor.Enter(lockDescriptor, ref lockTaken);

                    Debug.WriteLine($"Enter. New lock taken ({lockTaken}). Key={key}, ManagedThreadId={managedThreadId}");
                    newLock = true;
                }

                waiters = lockDescriptor.StartWait();
                Debug.WriteLine($"Enter. StartWait ({waiters}). Key={key}, ManagedThreadId={managedThreadId}");
            }

            if (!newLock)
            {
                Debug.WriteLine($"Enter. Start waiting for existing lock. Key={key}, ManagedThreadId={managedThreadId}");

                Monitor.Enter(lockDescriptor, ref lockTaken);
                Debug.WriteLine($"Enter. Existing lock taken ({lockTaken}). Key={key}, ManagedThreadId={managedThreadId}");
            }

            var workers = lockDescriptor.StartWork();
            Debug.WriteLine($"Enter. StartWork ({workers}). Key={key}, ManagedThreadId={managedThreadId}");
            waiters = lockDescriptor.EndWait();
            Debug.WriteLine($"Enter. EndWait ({waiters}). Key={key}, ManagedThreadId={managedThreadId}");

            Debug.WriteLine($"Enter. return {lockTaken}. Key={key}, ManagedThreadId={managedThreadId}");
            return lockTaken;
        }

        public void Exit(T key, bool lockTaken = true)
        {
            var managedThreadId = Thread.CurrentThread.ManagedThreadId;
            Debug.WriteLine($"Exit. Key={key}, ManagedThreadId={managedThreadId}");

            lock (_locks)
            {
                Debug.WriteLine($"Exit. lock (_locks). Key={key}, ManagedThreadId={managedThreadId}");

                if (_locks.TryGetValue(key, out var lockDescriptor))
                {
                    var workers = lockDescriptor.EndWork();
                    Debug.WriteLine($"Exit. EndWork ({workers}). Key={key}, ManagedThreadId={managedThreadId}");
                    if (lockTaken)
                    {
                        Monitor.Exit(lockDescriptor);
                        Debug.WriteLine($"Exit. Lock released ({Monitor.IsEntered(lockDescriptor)}). Key={key}, ManagedThreadId={managedThreadId}");
                    }
                    else
                    {
                        Debug.WriteLine($"Exit. Lock release skipped. Key={key}, ManagedThreadId={managedThreadId}");
                    }

                    if (lockDescriptor.WaitingThreads <= 0 && lockDescriptor.WorkingThreads <= 0)
                    {
                        _locks.Remove(key);
                        Debug.WriteLine($"Exit. LockDescriptor removed. Key={key}, ManagedThreadId={managedThreadId}");
                    }
                }
                else
                {
                    Debug.WriteLine($"Exit. LockDescriptor not found. Key={key}, ManagedThreadId={managedThreadId}");
                    throw new InvalidOperationException();
                }
            }

            Debug.WriteLine($"Exit. return. Key={key}, ManagedThreadId={managedThreadId}");
        }
    }
}