using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace DSFramework.Extensions
{
    public static class DictionaryExtensions
    {
        public static void AddRange<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, IEnumerable<KeyValuePair<TKey, TValue>> values)
        {
            if (dictionary == null || values == null)
            {
                return;
            }

            foreach (var pair in values)
            {
                dictionary.Add(pair.Key, pair.Value);
            }
        }

        public static void AppendRange<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, IEnumerable<KeyValuePair<TKey, TValue>> values)
        {
            if (dictionary == null || values == null)
            {
                return;
            }

            foreach (var pair in values)
            {
                dictionary[pair.Key] = pair.Value;
            }
        }

        public static bool DictionaryEqual<TKey, TValue>(
            this IDictionary<TKey, TValue> first, IDictionary<TKey, TValue> second)
        {
            return first.DictionaryEqual(second, null);
        }

        public static bool DictionaryEqual<TKey, TValue>(
            this IDictionary<TKey, TValue> first, IDictionary<TKey, TValue> second,
            IEqualityComparer<TValue> valueComparer)
        {
            if (ReferenceEquals(first, second)) return true;
            if ((first?.Count ?? 0) == 0 && (second?.Count ?? 0) == 0) return true;
            if (first == null || second == null) return false;
            if (first.Count != second.Count) return false;

            valueComparer = valueComparer ?? EqualityComparer<TValue>.Default;

            foreach (var kvp in first)
            {
                if (!second.TryGetValue(kvp.Key, out var secondValue)) return false;
                if (!valueComparer.Equals(kvp.Value, secondValue)) return false;
            }
            return true;
        }

        /// <summary>
        /// Gets a value from the dictionary with given key. Returns default value if can not find.
        /// </summary>
        /// <param name="dictionary">Dictionary to check and get</param>
        /// <param name="key">Key to find the value</param>
        /// <typeparam name="TKey">Type of the key</typeparam>
        /// <typeparam name="TValue">Type of the value</typeparam>
        /// <returns>Value if found, default if can not found.</returns>
        public static TValue GetOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key)
        {
            return dictionary.TryGetValue(key, out var obj) ? obj : default(TValue);
        }

        public static IReadOnlyDictionary<TKey, TValue> AsReadOnly<TKey, TValue>(
            this IDictionary<TKey, TValue> dictionary)
        {
            return new ReadOnlyDictionary<TKey, TValue>(dictionary);
        }
    }

}