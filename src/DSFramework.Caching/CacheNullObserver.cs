namespace DSFramework.Caching
{
    public class CacheNullObserver : ICacheObserver
    {
        public static readonly ICacheObserver Instance = new CacheNullObserver();

        public void KeysCount(string name, long count)
        {
        }

        public void OnGet(string name, bool missed)
        {
        }

        public void OnTouch(string name)
        {
        }

        public void OnAdd(string name)
        {
        }

        public void OnUpdate(string name)
        {
        }

        public void OnRemove(string name)
        {
        }

        public void OnCleanupBySize(string name, long removed)
        {
        }

        public void OnCleanupByTime(string name, long removed)
        {
        }
    }
}