using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace DSFramework.Threading
{
    public class ActionCachedAsyncExecutor
    {
        private readonly ILogger _logger;
        private readonly object _saveActionLocker = new object();
        private readonly SemaphoreSlim _semaphore;
        private readonly TaskFactory _taskFactory;

        private bool _actionExecuting;
        private Func<Task> _savedAction;

        public ActionCachedAsyncExecutor(ILogger logger)
        {
            _logger = logger;
            _taskFactory = Task.Factory;
        }

        public ActionCachedAsyncExecutor(TaskScheduler scheduler, ILogger logger, SemaphoreSlim semaphore = null)
        {
            _logger = logger;
            _semaphore = semaphore;
            _taskFactory = new TaskFactory(scheduler);
        }

        /// <summary>
        ///     Try executing action or save it and execute after executing current
        ///     Each next action rewrite last saved action
        /// </summary>
        /// <param name="action"></param>
        public void TryExecute(Action action)
            => TryExecute(() =>
            {
                action();
                return Task.FromResult(0);
            });

        /// <summary>
        ///     Try executing action or save it and execute after executing current
        ///     Each next action rewrite last saved action
        /// </summary>
        /// <param name="action"></param>
        public void TryExecute(Func<Task> action)
        {
            lock (_saveActionLocker)
            {
                if (_actionExecuting)
                {
                    _savedAction = action;
                    return;
                }

                _actionExecuting = true;
            }

            _taskFactory.StartNew(async () =>
            {
                do
                {
                    try
                    {
                        if (_semaphore != null)
                        {
                            await _semaphore.WaitAsync();
                        }

                        await action();
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(-1, ex, "async executor failed");
                    }
                    finally
                    {
                        _semaphore?.Release();
                    }

                    lock (_saveActionLocker)
                    {
                        _actionExecuting = _savedAction != null;
                        action = _savedAction;
                        _savedAction = null;
                    }
                }
                while (action != null);
            });
        }
    }
}