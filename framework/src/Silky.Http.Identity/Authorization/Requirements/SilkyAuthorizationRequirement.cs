using Microsoft.AspNetCore.Authorization;

namespace Silky.Http.Identity.Authorization.Requirements
{
    public class SilkyAuthorizationRequirement : IAuthorizationRequirement
    {
        public SilkyAuthorizationRequirement()
        {
        }

        public SilkyAuthorizationRequirement(string policyName)
        {
            PolicyName = policyName;
        }

        public string PolicyName { get; private set; }
    }
}