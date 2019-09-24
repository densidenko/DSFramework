using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DSFramework.Extensions;
using Microsoft.Extensions.Logging;

namespace DSFramework.Caching
{
    public class Cache<TKey, TValue> : ICache<TKey, TValue>
    {
        private readonly CacheSettings _cacheSettings;

        private readonly TimeSpan _cleanupInterval = TimeSpan.FromMinutes(10);

        private readonly Dictionary<TKey, CacheItem> _keys = new Dictionary<TKey, CacheItem>();
        private readonly object _lockObject = new object();
        private readonly ILogger _logger;

        private readonly string _name;
        private readonly Dictionary<CacheTag, CacheTagItem> _tags = new Dictionary<CacheTag, CacheTagItem>();

        private DateTime _lastCleanup = DateTime.UtcNow;

        public Cache(ILogger logger, string name = null, CacheSettings cacheSettings = null)
        {
            _logger = logger;
            _cacheSettings = cacheSettings ?? CacheSettings.Default;
            _name = name ?? GetType().ReadableName();
        }

        public Cache(ILogger logger, int expirationTimeoutMinutes, string name = null) : this(logger, name)
        {
            ExpirationTimeoutMinutes = expirationTimeoutMinutes;
        }

        public Cache(ILogger logger, int expirationTimeoutMinutes, int maxCapacity, string name = null) : this(logger,
            expirationTimeoutMinutes, name)
        {
            MaxCapacity = maxCapacity;
        }

        private ICacheObserver Observer => _cacheSettings.Observer ?? CacheNullObserver.Instance;

        /// <summary>
        ///     Max items in the cache.
        ///     If -1, then do not clear by capacity.
        /// </summary>
        public int MaxCapacity { get; } = 10000;

        /// <summary>
        ///     In minutes.
        ///     If -1, then do not clear by timeout.
        /// </summary>
        public int ExpirationTimeoutMinutes { get; } = 30;

        public int Count => _keys.Count;

        public bool IsEmpty => Count == 0;

        public TValue Get(TKey key)
        {
            Get(key, out var result);

            return result;
        }

        public bool Get(TKey key, out TValue value)
        {
            value = default(TValue);

            lock (_lockObject)
            {
                if (_keys.TryGetValue(key, out var item))
                {
                    Touch(item);

                    Observer.OnGet(_name, false);
                    value = item.Value;
                    return true;
                }
            }

            Observer.OnGet(_name, true);
            return false;
        }

        void ICache<TKey, TValue>.Update(TKey key, TValue value)
        {
            Update(key, value);
        }

        void ICache<TKey, TValue>.Add(TKey key, TValue value)
        {
            Add(key, value);
        }

        /// <summary>
        ///     Remove cach item by key
        /// </summary>
        /// <param name="key"></param>
        public void Remove(TKey key)
        {
            lock (_lockObject)
            {
                if (_keys.TryGetValue(key, out var item))
                {
                    Remove(item);
                }
            }
        }

        /// <summary>
        ///     Clear all cach
        /// </summary>
        public void Clear()
        {
            lock (_lockObject)
            {
                _keys.Clear();
                _tags.Clear();
                RefreshKeysCount();
            }
        }

        private void RefreshKeysCount()
        {
            Observer.KeysCount(_name, _keys.Count);
        }

        /// <summary>
        ///     Implementation getting with filling cach
        /// </summary>
        /// <param name="key"></param>
        /// <param name="creator">Method who executed in cach lock</param>
        /// <returns></returns>
        public TValue Get(TKey key, Func<TKey, TValue> creator)
        {
            return Get(key, (k, tags) => creator(k));
        }

        public TValue Get(TKey key, Func<TKey, List<CacheTag>, TValue> creator)
        {
            lock (_lockObject)
            {
                if (_keys.TryGetValue(key, out var item))
                {
                    Touch(item);

                    Observer.OnGet(_name, false);
                    return item.Value;
                }

                var tags = new List<CacheTag>();
                var newValue = creator(key, tags);
                Add(key, newValue, tags.ToArray());
                Observer.OnGet(_name, true);
                return newValue;
            }
        }

        public bool Contains(TKey key)
        {
            lock (_lockObject)
            {
                return _keys.ContainsKey(key);
            }
        }

        /// <summary>
        ///     Select by tags
        /// </summary>
        /// <param name="tag"></param>
        /// <returns></returns>
        public IEnumerable<KeyValuePair<TKey, TValue>> Select(CacheTag tag)
        {
            var result = new List<KeyValuePair<TKey, TValue>>();

            lock (_lockObject)
            {
                if (_tags.TryGetValue(tag, out var tagItem))
                {
                    while (tagItem != null)
                    {
                        result.Add(new KeyValuePair<TKey, TValue>(tagItem.Item.Key, tagItem.Item.Value));

                        Touch(tagItem.Item);

                        tagItem = tagItem.Next;
                    }
                }
            }

            return result;
        }

        /// <summary>
        ///     Update stored object and prolong its lifetime. If there is no object in cache does nothing.
        /// </summary>
        public void Update(TKey key, TValue value, params CacheTag[] tags)
        {
            lock (_lockObject)
            {
                if (_keys.ContainsKey(key))
                {
                    AddImpl(key, value, true, tags);
                }
            }
        }

        public void UpdateValue(TKey key, TValue value, params CacheTag[] tags)
        {
            UpdateValue(key, value, null, tags);
        }

        /// <summary>
        ///     Update value of the stored object. Doesn't prolong its lifetime. If there is no object in cache does nothing.
        /// </summary>
        public void UpdateValue(TKey key, TValue value, Func<TValue, TValue, bool> needUpdate, params CacheTag[] tags)
        {
            lock (_lockObject)
            {
                if (_keys.TryGetValue(key, out var current))
                {
                    if (needUpdate != null && !needUpdate(current.Value, value))
                    {
                        return;
                    }

                    AddImpl(key, value, false, tags);
                }
            }
        }

        /// <summary>
        ///     Add value into cache
        /// </summary>
        public void Add(TKey key, TValue value, params CacheTag[] tags) => AddImpl(key, value, true, tags);

        /// <summary>
        ///     Get all keys why cach containce now. Returned object is copy.
        /// </summary>
        /// <returns></returns>
        public List<TKey> GetKeys()
        {
            lock (_lockObject)
            {
                return _keys.Keys.ToList();
            }
        }

        public event Action<TKey, TValue> ItemRemoved;

        private void AddImpl(TKey key, TValue value, bool touch, params CacheTag[] tags)
        {
            lock (_lockObject)
            {
                var update = false;
                // first, remove old item
                if (_keys.TryGetValue(key, out var oldItem))
                {
                    update = true;
                    Remove(oldItem, false);
                }

                var item = new CacheItem
                {
                    Key = key,
                    Tags = tags,
                    Value = value
                };

                if (touch)
                {
                    Touch(item);
                }

                _keys[key] = item;

                if (MaxCapacity > 0 && _keys.Count > MaxCapacity)
                {
                    ClearLru();
                }

                if (tags != null)
                {
                    foreach (var tag in tags.Where(t => t != null))
                    {
                        AddTag(tag, item);
                    }
                }

                RefreshKeysCount();
                if (update)
                {
                    Observer.OnUpdate(_name);
                }
                else
                {
                    Observer.OnAdd(_name);
                }
            }

            if (DateTime.UtcNow - _lastCleanup >= _cleanupInterval)
            {
                _lastCleanup = DateTime.UtcNow;
                Task.Factory.StartNew(CleanupRoutine);
            }
        }

        private void OnItemRemoved(TKey key, TValue value)
        {
            try
            {
                ItemRemoved?.Invoke(key, value);
            }
            catch (Exception exc)
            {
                _logger.LogError("Error in OnItemRemoved", exc);
            }
        }

        private void Touch(CacheItem item)
        {
            // time
            item.LastTimeAccessed = DateTime.UtcNow;
            Observer.OnTouch(_name);
        }

        private void Remove(CacheItem item, bool observe = true)
        {
            if (item.Key != null) // null has first dummy item 
            {
                _keys.Remove(item.Key);
            }

            if (item.Tags != null)
            {
                foreach (var tag in item.Tags)
                {
                    RemoveTag(tag, item);
                }
            }

            if (item.Key != null)
            {
                OnItemRemoved(item.Key, item.Value);
            }

            RefreshKeysCount();

            if (observe)
            {
                Observer.OnRemove(_name);
            }
        }

        private void AddTag(CacheTag tag, CacheItem item)
        {
            var tagItem = new CacheTagItem { Item = item };

            if (_tags.TryGetValue(tag, out var firstTagItem))
            {
                tagItem.Next = firstTagItem;
            }

            _tags[tag] = tagItem;
        }

        private void RemoveTag(CacheTag tag, CacheItem item)
        {
            if (_tags.TryGetValue(tag, out var tagItem))
            {
                if (tagItem.Item == item) // if item on head
                {
                    if (tagItem.Next != null)
                    {
                        _tags[tag] = tagItem.Next;
                    }
                    else
                    {
                        _tags.Remove(tag);
                    }
                }

                while (tagItem.Next != null)
                {
                    if (tagItem.Next.Item == item)
                    {
                        tagItem.Next = tagItem.Next.Next;
                        break;
                    }

                    tagItem = tagItem.Next;
                }
            }
        }

        private void ClearLru()
        {
            if (MaxCapacity <= 0)
            {
                return;
            }

            _logger.LogDebug("Starting cleanup by capacity");

            // it is supposed that this.key is already locked
            var sortedItems = _keys.Values.OrderBy(i => i.LastTimeAccessed).ToList();

            var itemsToClean = Math.Min(MaxCapacity * 0.25, sortedItems.Count); // clear 25%
            var removed = 0;

            for (var i = 0; i < itemsToClean; i++)
            {
                if (sortedItems.Count > i)
                {
                    removed++;
                    Remove(sortedItems[i]);
                }
            }

            _logger.LogDebug($"Cleanup by capacity complete, items count = {_keys.Count}, removed = {removed}");
            Observer.OnCleanupBySize(_name, removed);
        }

        private void CleanupRoutine()
        {
            if (ExpirationTimeoutMinutes < 0)
            {
                return;
            }

            _logger.LogDebug("Starting cleanup by time");

            lock (_lockObject)
            {
                var items = _keys.Values.ToList();

                _logger.LogDebug($"Get lock, items count ={_keys.Count}");

                var removed = 0;
                var maxLastTimeAccessed = DateTime.MinValue;
                var minLastTimeAccessed = DateTime.MaxValue;
                var minTime = DateTime.UtcNow - TimeSpan.FromMinutes(ExpirationTimeoutMinutes);

                foreach (var item in items)
                {
                    var lastTimeAccessed = item.LastTimeAccessed;
                    if (lastTimeAccessed > maxLastTimeAccessed)
                    {
                        maxLastTimeAccessed = lastTimeAccessed;
                    }

                    if (lastTimeAccessed < minLastTimeAccessed)
                    {
                        minLastTimeAccessed = lastTimeAccessed;
                    }

                    if (item.LastTimeAccessed < minTime)
                    {
                        removed++;
                        Remove(item);
                    }
                }

                _logger.LogDebug(
                    $"Cleanup by time complete, items count ={_keys.Count}, removed = {removed}, maxLastTimeAccessed = {maxLastTimeAccessed.ToLocalTime()}, minLastTimeAccessed = {minLastTimeAccessed.ToLocalTime()}");
                Observer.OnCleanupByTime(_name, removed);

                _lastCleanup = DateTime.UtcNow;
            }
        }

        private class CacheItem
        {
            public TKey Key { get; set; }

            public CacheTag[] Tags { get; set; }

            public TValue Value { get; set; }

            public DateTime LastTimeAccessed { get; set; }

            public override string ToString()
            {
                return $"{{ {Key}, {Value} }}";
            }
        }

        private class CacheTagItem
        {
            public CacheItem Item { get; set; }
            public CacheTagItem Next { get; set; }
        }
    }
}