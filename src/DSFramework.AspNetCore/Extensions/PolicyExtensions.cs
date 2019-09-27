using Polly;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DSFramework.AspNetCore.Extensions
{
    public static class PolicyExtensions
    {
        public static IEnumerable<Task> RunInBulkhead<T>(this IEnumerable<T> source, Func<T, Task> func, int maxParallelization)
        {
            var enumerable = source as T[] ?? source.ToArray();
            var policy = Policy.BulkheadAsync(maxParallelization, enumerable.Count());
            var tasks = enumerable.Select(item => policy.ExecuteAsync(() => func(item))).ToArray();
            return tasks;
        }
        public static IEnumerable<Task<TOut>> RunInBulkhead<TIn, TOut>(this IEnumerable<TIn> source, Func<TIn, Task<TOut>> func, int maxParallelization)
        {
            var enumerable = source.ToList();
            var policy = Policy.BulkheadAsync(maxParallelization, enumerable.Count());
            var tasks = enumerable.Select(item => policy.ExecuteAsync(() => func(item))).ToArray();
            return tasks;
        }
    }
}