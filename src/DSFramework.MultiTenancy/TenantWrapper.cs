using System;
using DSFramework.Dependency;

namespace DSFramework.MultiTenancy
{
    public class TenantWrapper : ITenant, IScopedDependency
    {
        private readonly TenantInfo _tenant;

        public TenantInfo Value => _tenant ?? throw new InvalidOperationException();
        public bool HasValue => _tenant != null;

        public TenantWrapper(TenantInfo tenant)
        {
            _tenant = tenant;
        }
    }
}