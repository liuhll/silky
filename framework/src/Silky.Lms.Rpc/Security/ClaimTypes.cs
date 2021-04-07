namespace Silky.Lms.Rpc.Security
{
    public class ClaimTypes
    {
        public const string UserId = "http://lms.com/identity/claims/userid";

        public const string UserName = "http://lms.com/identity/claims/username";

        public const string Email = "http://lms.com/identity/claims/email";

        public const string Phone = "http://lms.com/identity/claims/phone";

        public const string Terminal = "http://lms.com/identity/claims/terminal";

        public const string OrgId = "http://lms.com/identity/claims/OrgId";

        public const string DataPermission = "http://lms.com/identity/claims/DataPermission";

        public const string DataPermissionOrgIds = "http://lms.com/identity/claims/DataPermissionOrgIds";

        public const string TenantId = "http://lms.com/identity/claims/TenantId";
        
        public const string IsAllOrg = "http://lms.com/identity/claims/IsAllOrg";

        public const string Expired = "exp";
    }
}