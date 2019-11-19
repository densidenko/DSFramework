using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DSFramework.Security.Authorization.Extensions;
using Microsoft.AspNetCore.Authorization;

namespace DSFramework.Web.AspNetCore.Authorization
{
    public sealed class PermissionAuthorizationRequirement : AuthorizationHandler<PermissionAuthorizationRequirement>, IAuthorizationRequirement
    {
        public IEnumerable<string> Permissions { get; }

        public PermissionAuthorizationRequirement(IEnumerable<string> permissions)
        {
            Permissions = permissions ?? throw new ArgumentNullException(nameof(permissions));
        }

        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionAuthorizationRequirement requirement)
        {
            if (context.User == null || requirement.Permissions == null || !requirement.Permissions.Any())
            {
                return Task.CompletedTask;
            }

            var hasPermission = requirement.Permissions.Any(permission => context.User.HasPermission(permission));

            if (!hasPermission)
            {
                return Task.CompletedTask;
            }

            context.Succeed(requirement);

            return Task.CompletedTask;
        }
    }
}