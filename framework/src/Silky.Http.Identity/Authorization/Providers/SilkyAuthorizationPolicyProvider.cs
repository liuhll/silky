using System;
using System.Linq;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using Silky.Http.Identity.Authorization.Requirements;
using Silky.Rpc.Runtime.Server;

namespace Silky.Http.Identity.Authorization.Providers;

public sealed class SilkyAuthorizationPolicyProvider : DefaultAuthorizationPolicyProvider, IAuthorizationPolicyProvider
{
    private readonly IServiceEntryManager _serviceEntryManager;

    public SilkyAuthorizationPolicyProvider(
        [NotNull] [ItemNotNull] IOptions<AuthorizationOptions> options,
        IServiceEntryManager serviceEntryManager) :
        base(options)
    {
        _serviceEntryManager = serviceEntryManager;
    }

    public override async Task<AuthorizationPolicy> GetPolicyAsync(string policyName)
    {
        var policy = await base.GetPolicyAsync(policyName);
        if (policy != null)
        {
            return policy;
        }

        var serviceEntries = _serviceEntryManager.GetAllEntries();
        if (serviceEntries.Any(se => se.AuthorizeData.Any(ad => ad.Policy == policyName)))
        {
            //TODO: Optimize & Cache!
            var policyBuilder = new AuthorizationPolicyBuilder(Array.Empty<string>());
            policyBuilder.Requirements.Add(new PermissionRequirement(policyName));
            return policyBuilder.Build();
        }

        return null;
    }
}