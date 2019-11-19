using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DSFramework.Threading
{
    public class GenericLock<TKey> where TKey : IComparable<TKey>
    {
        internal class SemaphoreHolder
        {
            private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);
            private long _awaiters;
            public TKey Key { get; }

            internal event EventHandler Released;

            public SemaphoreHolder(TKey key)
            {
                Key = key;
            }

            protected virtual void OnReleased() => Released?.Invoke(this, EventArgs.Empty);

            public void EnqueueAwaiter() => Interlocked.Increment(ref _awaiters);

            public void DequeueAwaiter() => Interlocked.Decrement(ref _awaiters);

            public long GetCurrentAwaiters() => Interlocked.Read(ref _awaiters);

            public void Release()
            {
                _semaphore.Release();
                OnReleased();
            }

            public async Task WaitAsync() => await _semaphore.WaitAsync();

            public void Wait() => _semaphore.Wait();
        }

        private class LockHolder : IDisposable
        {
            private readonly SemaphoreHolder _holder;

            public LockHolder(SemaphoreHolder holder)
            {
                _holder = holder;
            }

            public void Dispose() => _holder.Release();
        }

        private class LocksHolder : IDisposable
        {
            private readonly IEnumerable<IDisposable> _locks;

            public LocksHolder(IEnumerable<IDisposable> locks)
            {
                _locks = locks;
            }

            public void Dispose()
            {
                foreach (var holder in _locks)
                {
                    holder.Dispose();
                }
            }
        }

        private readonly Dictionary<TKey, SemaphoreHolder> _semaphores = new Dictionary<TKey, SemaphoreHolder>();
        private readonly object _lock = new object();

        public async Task<IDisposable> LockAsync(TKey key)
        {
            SemaphoreHolder holder;

            lock (_lock)
            {
                if (!_semaphores.TryGetValue(key, out holder))
                {
                    holder = new SemaphoreHolder(key);
                    holder.Released += Holder_Released;
                    _semaphores[key] = holder;
                }

                holder.EnqueueAwaiter();
            }

            await holder.WaitAsync();

            return new LockHolder(holder);
        }

        public async Task<IDisposable> LockAsync(IEnumerable<TKey> keys)
        {
            var locks = new List<IDisposable>();
            try
            {
                foreach (var key in keys.Distinct().OrderBy(key => key))
                {
                    locks.Add(await LockAsync(key));
                }
            }
            catch
            {
                foreach (var @lock in locks)
                {
                    @lock.Dispose();
                }

                throw;
            }

            return new LocksHolder(locks);
        }

        public IDisposable Lock(TKey key) => LockAsync(key).Result;

        public IDisposable Lock(IEnumerable<TKey> keys) => LockAsync(keys).Result;

        private void Holder_Released(object sender, EventArgs e)
        {
            if (sender is SemaphoreHolder holder)
            {
                lock (_lock)
                {
                    holder.DequeueAwaiter();

                    if (holder.GetCurrentAwaiters() == 0)
                    {
                        holder.Released -= Holder_Released;
                        _semaphores.Remove(holder.Key);
                    }
                }
            }
        }
    }
}