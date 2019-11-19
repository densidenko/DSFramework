using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace DSFramework.Threading
{
    public class GenericExecutor<TKey>
    {
        private readonly Dictionary<TKey, ActionCachedAsyncExecutor> _executors = new Dictionary<TKey, ActionCachedAsyncExecutor>();
        private readonly ILogger _logger;
        private readonly SemaphoreSlim _semaphore;
        private readonly TaskScheduler _taskScheduler;

        public GenericExecutor(int maxSimultaneousExecutions = 100)
            : this(NullLogger.Instance, TaskScheduler.Default, maxSimultaneousExecutions)
        { }

        public GenericExecutor(ILogger logger, int maxSimultaneousExecutions = 100)
            : this(logger, TaskScheduler.Default, maxSimultaneousExecutions)
        { }

        public GenericExecutor(ILogger logger, SemaphoreSlim semaphore)
            : this(logger, TaskScheduler.Default, semaphore)
        { }

        public GenericExecutor(TaskScheduler taskScheduler, int maxSimultaneousExecutions = 100)
            : this(NullLogger.Instance, taskScheduler, new SemaphoreSlim(maxSimultaneousExecutions, maxSimultaneousExecutions))
        { }

        public GenericExecutor(ILogger logger, TaskScheduler taskScheduler, int maxSimultaneousExecutions = 100)
            : this(logger, taskScheduler, new SemaphoreSlim(maxSimultaneousExecutions, maxSimultaneousExecutions))
        { }

        public GenericExecutor(ILogger logger, TaskScheduler taskScheduler, SemaphoreSlim semaphore)
        {
            _logger = logger;
            _taskScheduler = taskScheduler;
            _semaphore = semaphore;
        }

        public void TryExecute(TKey key, Action action)
        {
            var executor = GetExecutor(key);

            executor.TryExecute(action);
        }

        public void TryExecute(TKey key, Func<Task> action)
        {
            var executor = GetExecutor(key);

            executor.TryExecute(action);
        }

        private ActionCachedAsyncExecutor GetExecutor(TKey key)
        {
            lock (_executors)
            {
                if (!_executors.TryGetValue(key, out var executor))
                {
                    executor = new ActionCachedAsyncExecutor(_taskScheduler, _logger, _semaphore);
                    _executors[key] = executor;
                }

                return executor;
            }
        }
    }
}