using System;
using System.Collections.Generic;

namespace DSFramework.MultiTenancy
{
    public class TenantContext : IDisposable
    {
        private bool _disposed;

        public string Id { get; } = Guid.NewGuid().ToString();

        public TenantInfo Tenant { get; }

        public IDictionary<string, object> Properties { get; }

        public TenantContext(TenantInfo tenant)
        {
            Tenant = tenant ?? throw new ArgumentNullException(nameof(tenant));
            Properties = new Dictionary<string, object>();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
            {
                foreach (var prop in Properties)
                {
                    TryDisposeProperty(prop.Value as IDisposable);
                }
            }

            // release any unmanaged objects
            // set the object references to null

            _disposed = true;
        }

        private static void TryDisposeProperty(IDisposable obj)
        {
            if (obj == null)
                return;

            try
            {
                obj.Dispose();
            }
            catch (ObjectDisposedException)
            {
            }
        }

        ~TenantContext()
        {
            Dispose(false);
        }
    }
}