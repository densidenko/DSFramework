using System;
using System.Collections.Generic;
using System.Linq;

namespace DSFramework.Extensions
{
    public static class CommonExtensions
    {
        public static bool In<T>(this T val, params T[] values)
        {
            return values.Contains(val);
        }

        public static bool IsBetween<T>(this T item, T start, T end) where T : IComparable<T>
        {
            return
                Comparer<T>.Default.Compare(item, start) >= 0 &&
                Comparer<T>.Default.Compare(item, end) <= 0;
        }

        public static bool SafeSequenceEqual<TSource>(this IEnumerable<TSource> first, IEnumerable<TSource> second, IEqualityComparer<TSource> comparer = null)
        {
            // if both null or 1 object
            if (ReferenceEquals(first, second)) { return true; }
            // one is null
            if (ReferenceEquals(null, second)) return false;
            if (ReferenceEquals(null, first)) return false;

            return first.SequenceEqual(second, comparer);
        }
    }

}