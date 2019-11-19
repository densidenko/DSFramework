using System.Security.Claims;

namespace DSFramework.Security.Authorization
{
    public class UserClaimTypes
    {
        public const string USER_NAME = ClaimTypes.Name;
        public const string USER_ID = ClaimTypes.NameIdentifier;
        public const string SERIAL_NUMBER = ClaimTypes.SerialNumber;
        public const string ROLE = ClaimTypes.Role;
        public const string DISPLAY_NAME = "DisplayName";
        public const string TENANT_ID = "TenantId";
        public const string PERMISSION = "Permission";
        public const string PACKED_PERMISSION = "PackedPermission";
        public const string IMPERSONATOR_USER_ID = "ImpersonatorUserId";
        public const string IMPERSONATOR_TENANT_ID = "ImpersonatorTenantId";
    }
}