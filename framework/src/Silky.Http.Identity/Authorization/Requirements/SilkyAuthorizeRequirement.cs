using Microsoft.AspNetCore.Authorization;

namespace Silky.Http.Identity.Authorization.Requirements;

public class SilkyAuthorizeRequirement : IAuthorizationRequirement
{
    public SilkyAuthorizeRequirement(params string[] policies)
    {
        Policies = policies;
    }
    public string[] Policies { get; private set; }
    
}