using System;

namespace DSFramework.Authorization
{
    public class PermissionDependencyContext
    {
        public IServiceProvider ServiceProvider { get; }

        public PermissionDependencyContext(IServiceProvider provider)
        {
            ServiceProvider = provider ?? throw new ArgumentNullException(nameof(provider));
        }
    }
}