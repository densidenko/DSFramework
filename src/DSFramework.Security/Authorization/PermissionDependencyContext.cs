using System;

namespace DSFramework.Security.Authorization
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