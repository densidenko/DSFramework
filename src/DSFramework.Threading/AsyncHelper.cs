using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace DSFramework.Threading
{
    public static class AsyncHelper
    {
        private static readonly TaskFactory _taskFactory =
            new TaskFactory(CancellationToken.None, TaskCreationOptions.None, TaskContinuationOptions.None, TaskScheduler.Default);

        /// <summary>
        ///     Checks if given method is an async method.
        /// </summary>
        /// <param name="method">A method to check</param>
        public static bool IsAsync(this MethodInfo method)
            => method.ReturnType == typeof(Task) ||
               method.ReturnType.GetTypeInfo().IsGenericType && method.ReturnType.GetGenericTypeDefinition() == typeof(Task<>);

        public static TResult RunSync<TResult>(Func<Task<TResult>> func) => _taskFactory.StartNew(func).Unwrap().GetAwaiter().GetResult();

        public static void RunSync(Func<Task> func) => _taskFactory.StartNew(func).Unwrap().GetAwaiter().GetResult();
    }
}