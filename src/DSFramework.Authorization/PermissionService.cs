using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using DSFramework.Dependency;
using DSFramework.Exceptions;
using DSFramework.Extensions;
using DSFramework.Functional;
using Microsoft.Extensions.DependencyInjection;

namespace DSFramework.Authorization
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

        public Maybe<Permission> Find(string name)
        {
            var permission = _permissions.GetOrDefault(name);

            return permission;
        }

        public IReadOnlyList<Permission> ReadList()
        {
            return _permissions.Values.ToImmutableList();
        }
    }
}