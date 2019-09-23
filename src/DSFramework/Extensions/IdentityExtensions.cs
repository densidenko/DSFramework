﻿using System;
using System.Globalization;
using System.Security.Claims;
using System.Security.Principal;
using DSFramework.Runtime;

namespace DSFramework.Extensions
{
    public static class IdentityExtensions
    {
        public static T FindUserId<T>(this IIdentity identity)
            where T : IConvertible
        {
            var id = identity?.FindUserClaimValue(UserClaimTypes.USER_ID);

            if (id != null)
                return (T)Convert.ChangeType(id, typeof(T), CultureInfo.InvariantCulture);

            return default;
        }

        public static long? FindUserId(this IIdentity identity)
        {
            var id = identity?.FindUserClaimValue(UserClaimTypes.USER_ID);

            if (id.IsEmpty())
                return null;

            if (!long.TryParse(id, out var userId))
                return null;

            return userId;
        }

        public static long? FindTenantId(this IIdentity identity)
        {
            var tenantClaim = identity?.FindUserClaimValue(UserClaimTypes.TENANT_ID);

            if (tenantClaim.IsEmpty())
                return default;

            return !long.TryParse(tenantClaim, out var tenantId) ? default : tenantId;
        }

        public static long? FindImpersonatorTenantId(this IIdentity identity)
        {
            var tenantClaim = identity?.FindUserClaimValue(UserClaimTypes.IMPERSONATOR_TENANT_ID);

            if (tenantClaim.IsEmpty())
                return default;

            return !long.TryParse(tenantClaim, out var tenantId) ? default : tenantId;
        }

        public static long? FindImpersonatorUserId(this IIdentity identity)
        {
            var tenantClaim = identity?.FindUserClaimValue(UserClaimTypes.IMPERSONATOR_USER_ID);

            if (tenantClaim.IsEmpty())
                return default;

            return !long.TryParse(tenantClaim, out var tenantId) ? default : tenantId;
        }

        public static string FindFirstValue(this ClaimsIdentity identity, string claimType)
        {
            return identity?.FindFirst(claimType)?.Value;
        }

        public static string FindUserClaimValue(this IIdentity identity, string claimType)
        {
            return (identity as ClaimsIdentity)?.FindFirstValue(claimType);
        }

        public static string FindUserDisplayName(this IIdentity identity)
        {
            var displayName = identity?.FindUserClaimValue(UserClaimTypes.USER_NAME);
            return string.IsNullOrWhiteSpace(displayName) ? FindUserName(identity) : displayName;
        }

        public static string FindUserName(this IIdentity identity)
        {
            return identity?.FindUserClaimValue(UserClaimTypes.USER_NAME);
        }
    }
}