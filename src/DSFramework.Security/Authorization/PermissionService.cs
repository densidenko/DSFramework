using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using DSFramework.Exceptions;
using DSFramework.Extensions;
using DSFramework.Extensions.DependencyInjection;
using DSFramework.Functional;
using Microsoft.Extensions.DependencyInjection;

namespace DSFramework.Security.Authorization
{
    internal class PermissionService : IPermissionService, ISingletonDependency
    {
        private readonly IServiceProvider _provider;
        private readonly PermissionDictionary _permissions = new PermissionDictionary();

        public PermissionService(IServiceProvider provider)
        {
            _provider = provider ?? throw new ArgumentNullException(nameof(provider));

            Initialize();
        }

        public Maybe<Permission> Find(string name)
        {
            var permission = _permissions.GetOrDefault(name);

            return permission;
        }

        public IReadOnlyList<Permission> ReadList() => _permissions.Values.ToImmutableList();

        private void Initialize()
        {
            using (var scope = _provider.CreateScope())
            {
                var providers = scope.ServiceProvider.GetServices<IAuthorizationProvider>();
                foreach (var provider in providers)
                {
                    var permissions = provider.ProvidePermissions();
                    foreach (var permission in permissions)
                    {
                        if (_permissions.ContainsKey(permission.Name))
                        {
                            throw new DSFrameworkException("There is already a permission with name: " + permission.Name);
                        }

                        _permissions[permission.Name] = permission;
                    }
                }
            }

            _permissions.AddAllPermissions();
        }
    }
}