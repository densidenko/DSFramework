namespace DSFramework.Caching
{
    public interface ICacheObserver
    {
        void KeysCount(string name, long count);
        void OnGet(string name, bool missed);
        void OnTouch(string name);
        void OnAdd(string name);
        void OnUpdate(string name);
        void OnRemove(string name);
        void OnCleanupBySize(string name, long removed);
        void OnCleanupByTime(string name, long removed);
    }
}