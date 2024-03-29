﻿using System.Collections.Generic;
using DSFramework.Extensions.DependencyInjection;

namespace DSFramework.Security.Authorization
{
    /// <summary>
    ///     This is the main interface to define permissions for an application.
    ///     Implement it to define permissions for your module.
    /// </summary>
    public interface IAuthorizationProvider : ITransientDependency
    {
        /// <summary>
        ///     This method is called once on application startup to allow to define permissions.
        /// </summary>
        IEnumerable<Permission> ProvidePermissions();
    }
}