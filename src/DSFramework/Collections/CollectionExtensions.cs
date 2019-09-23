using System;
using System.Collections.Generic;
using System.Linq;

namespace DSFramework.Collections
{
    /// <summary>
    ///     Extension methods for Collections.
    /// </summary>
    public static class CollectionExtensions
    {
        /// <summary>
        ///     Checks whatever given collection object is null or has no item.
        /// </summary>
        public static bool IsNullOrEmpty<T>(this ICollection<T> source)
        {
            return source == null || source.Count <= 0;
        }

        /// <summary>
        ///     Adds an item to the collection if it's not already in the collection.
        /// </summary>
        /// <param name="source">Collection</param>
        /// <param name="item">Item to check and add</param>
        /// <typeparam name="T">Type of the items in the collection</typeparam>
        /// <returns>Returns True if added, returns False if not.</returns>
        public static bool AddIfNotContains<T>(this ICollection<T> source, T item)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }

            if (source.Contains(item))
            {
                return false;
            }

            source.Add(item);
            return true;
        }

        public static void AddRange<T>(this ICollection<T> initial, IEnumerable<T> other)
        {
            if (other == null)
                return;

            if (initial is List<T> list)
            {
                list.AddRange(other);
                return;
            }

            foreach (var item in other)
            {
                initial.Add(item);
            }
        }

        public static bool ListEquals<T>(this IEnumerable<T> first, IEnumerable<T> second)
        {
            var firstList = first?.ToList();
            var secondList = second?.ToList();
            if (ReferenceEquals(firstList, secondList))
                return true;
            if ((firstList?.Count ?? 0) == 0 && (secondList?.Count ?? 0) == 0)
                return true;
            if (firstList == null || secondList == null)
                return false;
            if (firstList.Count != secondList.Count)
                return false;
            return firstList.All(secondList.Contains) && firstList.Count == secondList.Count;
        }
    }
}