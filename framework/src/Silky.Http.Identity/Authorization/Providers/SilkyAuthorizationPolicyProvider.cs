using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace Silky.Http.Identity.Authorization.Providers
{
    internal sealed class SilkyAuthorizationPolicyProvider : IAuthorizationPolicyProvider
    {
        public DefaultAuthorizationPolicyProvider FallbackPolicyProvider { get; }
        
        public Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
        {
            return FallbackPolicyProvider.GetPolicyAsync(policyName);
        }

        public Task<AuthorizationPolicy> GetDefaultPolicyAsync()
        {
            return FallbackPolicyProvider.GetFallbackPolicyAsync();
        }

        public Task<AuthorizationPolicy?> GetFallbackPolicyAsync()
        {
            return FallbackPolicyProvider.GetDefaultPolicyAsync();
        }
    }
}