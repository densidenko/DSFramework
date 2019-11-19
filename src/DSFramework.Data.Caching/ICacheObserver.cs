namespace DSFramework.Data.Caching
{
    public interface ICacheObserver
    {
        void KeysCount(string name, long count);
        void OnAdd(string name);
        void OnCleanupBySize(string name, long removed);
        void OnCleanupByTime(string name, long removed);
        void OnGet(string name, bool missed);
        void OnRemove(string name);
        void OnTouch(string name);
        void OnUpdate(string name);
    }
}