using DSFramework.Domain.Abstractions.Repositories.Observers;

namespace DSFramework.Domain.Abstractions.Repositories
{
    public static class RepositoryGlobalSettings
    {
        public static IRepositoryObserver Observer { get; private set; }

        public static void RegisterObserver<T>() where T : IRepositoryObserver, new()
        {
            Observer = new T();
        }
    }
}