namespace DSFramework.Data.Caching
{
    public class CacheSettings
    {
        public static CacheSettings Default = new CacheSettings();
        public ICacheObserver Observer { get; set; } = CacheGlobalSettings.Observer;

        public CacheSettings WithObserver<T>() where T : ICacheObserver, new()
        {
            Observer = new T();
            return this;
        }
    }
}