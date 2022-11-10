using System;
using System.Linq;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Silky.Http.Identity.Authorization.Requirements;
using Silky.Rpc.Extensions;
using Silky.Rpc.Runtime.Server;

namespace Silky.Http.Identity.Authorization.Providers;

internal sealed class SilkyAuthorizationPolicyProvider : DefaultAuthorizationPolicyProvider,
    IAuthorizationPolicyProvider
{
    private readonly IServerManager _serverManager;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public SilkyAuthorizationPolicyProvider(
        [NotNull] [ItemNotNull] IOptions<AuthorizationOptions> options,
        IHttpContextAccessor httpContextAccessor) :
        base(options)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public override async Task<AuthorizationPolicy> GetPolicyAsync(string policyName)
    {
        var policy = await base.GetPolicyAsync(policyName);
        if (policy != null)
        {
            return policy;
        }

        var serviceEntry = _httpContextAccessor.HttpContext.GetServiceEntry();
        if (serviceEntry?.AuthorizeData.Any(ad => ad.Policy == policyName) == true)
        {
            var policyBuilder = new AuthorizationPolicyBuilder(Array.Empty<string>());
            policyBuilder.Requirements.Add(new PermissionRequirement(policyName));
            return policyBuilder.Build();
        }

        var serviceEntryDescriptor = _httpContextAccessor.HttpContext.GetServiceEntryDescriptor();
        if (serviceEntryDescriptor?.AuthorizeData.Any(ad => ad.Policy == policyName) == true)
        {
            var policyBuilder = new AuthorizationPolicyBuilder(Array.Empty<string>());
            policyBuilder.Requirements.Add(new PermissionRequirement(policyName));
            return policyBuilder.Build();
        }

        return null;
    }
}