using System;
using System.Collections.Generic;
using System.Security.Claims;

namespace DSFramework.Runtime.Session
{
    public class NullUserSession : IUserSession
    {
        public bool IsAuthenticated => false;
        public long? UserId => null;
        public string UserName => string.Empty;
        public IReadOnlyList<string> Permissions => new List<string>();
        public IReadOnlyList<string> Roles => new List<string>();
        public IReadOnlyList<Claim> Claims => new List<Claim>();
        public string UserDisplayName => string.Empty;
        public string UserBrowserName => string.Empty;
        public string UserIP => string.Empty;
        public long? ImpersonatorUserId => null;
        public bool IsInRole(string role)
        {
            return false;
        }

        public bool IsGranted(string permission)
        {
            return false;
        }
    }
}