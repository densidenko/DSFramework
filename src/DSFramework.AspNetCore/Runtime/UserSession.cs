using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using DSFramework.AspNetCore.Http;
using DSFramework.Authorization.Extensions;
using DSFramework.Extensions;
using DSFramework.GuardToolkit;
using DSFramework.Runtime.Session;
using Microsoft.AspNetCore.Http;

namespace DSFramework.AspNetCore.Runtime
{
    public class UserSession : IUserSession
    {
        private readonly IHttpContextAccessor _context;

        public UserSession(IHttpContextAccessor context)
        {
            _context = Check.NotNull(context, nameof(context));
        }

        public bool IsAuthenticated => _context?.HttpContext?.User?.Identity.IsAuthenticated ?? false;
        public long? UserId => _context?.HttpContext?.User?.Identity.FindUserId();
        public string UserName => _context?.HttpContext?.User?.Identity.Name;
        public IReadOnlyList<string> Permissions => _context?.HttpContext?.User?.FindPermissions();
        public IReadOnlyList<string> Roles => _context?.HttpContext?.User?.FindRoles();
        public IReadOnlyList<Claim> Claims => _context?.HttpContext?.User?.Claims?.ToList();
        public string UserDisplayName => _context?.HttpContext?.User?.Identity.FindUserDisplayName();
        public string UserBrowserName => _context.HttpContext?.GetUserAgent();
        public string UserIP => _context.HttpContext?.GetIp();
        public long? ImpersonatorUserId => _context?.HttpContext?.User?.Identity.FindImpersonatorUserId();
        public bool IsInRole(string role)
        {
            if (!IsAuthenticated) throw new InvalidOperationException("This operation need user authenticated");

            return _context.HttpContext.User.IsInRole(role);
        }

        public bool IsGranted(string permission)
        {
            if (!IsAuthenticated) throw new InvalidOperationException("This operation need user authenticated");

            return _context.HttpContext.User.HasPermission(permission);
        }
    }
}