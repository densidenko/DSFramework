namespace DSFramework.Caching
{
    public interface ICache<in TKey, TValue>
    {
        bool Get(TKey key, out TValue value);

        /// <summary>
        ///     Add value into cache
        /// </summary>
        void Update(TKey key, TValue value);

        /// <summary>
        ///     Add value into cache
        /// </summary>
        void Add(TKey key, TValue value);

        /// <summary>
        ///     Remove cache item by key
        /// </summary>
        /// <param name="key"></param>
        void Remove(TKey key);

        /// <summary>
        ///     Clear all cache
        /// </summary>
        void Clear();
    }
}