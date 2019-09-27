using System;
using DSFramework.Authorization;
using Microsoft.AspNetCore.Authorization;

namespace DSFramework.AspNetCore.Authorization
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = false)]
    public class PermissionAuthorizeAttribute : AuthorizeAttribute
    {
        /// <summary>
        ///     Creates a new instance of <see cref="AuthorizeAttribute" /> class.
        /// </summary>
        /// <param name="permissions">A list of permissions to authorize</param>
        public PermissionAuthorizeAttribute(params string[] permissions)
        {
            Policy = $"{PermissionConstant.POLICY_PREFIX}{string.Join(PermissionConstant.POLICY_NAME_SPLIT_SYMBOL, permissions)}";
        }
    }
}