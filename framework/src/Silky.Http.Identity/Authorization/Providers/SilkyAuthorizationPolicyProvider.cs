using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using Silky.Http.Identity.Authorization.Requirements;

namespace Silky.Http.Identity.Authorization.Providers
{
    internal sealed class SilkyAuthorizationPolicyProvider : IAuthorizationPolicyProvider
    {
        private readonly DefaultAuthorizationPolicyProvider _authorizationPolicyProvider;

        public SilkyAuthorizationPolicyProvider(IOptions<AuthorizationOptions> options)
        {
            _authorizationPolicyProvider = new DefaultAuthorizationPolicyProvider(options);
        }

        public Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
        {
            return _authorizationPolicyProvider.GetPolicyAsync(policyName);
        }

        public Task<AuthorizationPolicy> GetDefaultPolicyAsync()
        {
            return _authorizationPolicyProvider.GetDefaultPolicyAsync();
        }

        public Task<AuthorizationPolicy?> GetFallbackPolicyAsync()
        {
            return _authorizationPolicyProvider.GetDefaultPolicyAsync();
        }
    }
}