namespace DSFramework.Data.Caching
{
    public interface ICache<in TKey, TValue>
    {
        /// <summary>
        ///     Add value into cache
        /// </summary>
        void Add(TKey key, TValue value);

        /// <summary>
        ///     Clear all cache
        /// </summary>
        void Clear();

        bool Get(TKey key, out TValue value);

        /// <summary>
        ///     Remove cache item by key
        /// </summary>
        /// <param name="key"></param>
        void Remove(TKey key);

        /// <summary>
        ///     Add value into cache
        /// </summary>
        void Update(TKey key, TValue value);
    }
}