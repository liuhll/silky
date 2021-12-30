using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using Silky.Http.Identity.Authorization.Requirements;
using Silky.Rpc.Security;

namespace Silky.Http.Identity.Authorization.Providers;

public sealed class SilkyAuthorizationPolicyProvider : IAuthorizationPolicyProvider
{
    public DefaultAuthorizationPolicyProvider FallbackPolicyProvider { get; }

    public SilkyAuthorizationPolicyProvider(IOptions<AuthorizationOptions> options)
    {
        FallbackPolicyProvider = new DefaultAuthorizationPolicyProvider(options);
    }


    public Task<AuthorizationPolicy> GetDefaultPolicyAsync()
    {
        return FallbackPolicyProvider.GetDefaultPolicyAsync();
    }


    public Task<AuthorizationPolicy> GetFallbackPolicyAsync()
    {
        return FallbackPolicyProvider.GetFallbackPolicyAsync();
    }


    public Task<AuthorizationPolicy> GetPolicyAsync(string policyName)
    {
        if (policyName.StartsWith(AuthorizationConsts.SilkyAuthorizePrefix))
        {
            var policies = policyName[AuthorizationConsts.SilkyAuthorizePrefix.Length..]
                .Split(',', StringSplitOptions.RemoveEmptyEntries);


            var policy = new AuthorizationPolicyBuilder();
            policy.AddRequirements(new SilkyAuthorizeRequirement(policies));

            return Task.FromResult(policy.Build());
        }
        
        return FallbackPolicyProvider.GetPolicyAsync(policyName);
    }
}