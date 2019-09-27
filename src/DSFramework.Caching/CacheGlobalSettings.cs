namespace DSFramework.Caching
{
    public static class CacheGlobalSettings
    {
        public static ICacheObserver Observer { get; private set; }

        public static void RegisterObserver<T>() where T : ICacheObserver, new()
        {
            Observer = new T();
        }
    }
}