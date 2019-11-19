using System;
using System.Collections.Generic;

namespace DSFramework.Security.Authorization.Extensions
{
    public static class PermissionExtensions
    {
        public static string PackPermissionsToString(this IEnumerable<string> permissions)
            => string.Join(PermissionConstant.PACKING_SYMBOL, permissions);

        public static IEnumerable<string> UnpackPermissionsFromString(this string packedPermissions)
        {
            if (packedPermissions == null) throw new ArgumentNullException(nameof(packedPermissions));

            return packedPermissions.Split(new[] { PermissionConstant.PACKING_SYMBOL }, StringSplitOptions.None);
        }

        public static IEnumerable<string> ExtractPermissionsFromPolicyName(this string policyName)
            => policyName.Substring(PermissionConstant.POLICY_PREFIX.Length)
                         .Split(new[] { PermissionConstant.POLICY_NAME_SPLIT_SYMBOL }, StringSplitOptions.None);
    }
}