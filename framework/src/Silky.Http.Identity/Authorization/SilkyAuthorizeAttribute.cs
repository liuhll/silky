using System;
using Silky.Rpc.Security;

namespace Microsoft.AspNetCore.Authorization;

[AttributeUsage(AttributeTargets.Interface | AttributeTargets.Class | AttributeTargets.Method)]
public class SilkyAuthorizeAttribute : AuthorizeAttribute
{
    public SilkyAuthorizeAttribute(params string[] policies)
    {
        if (policies != null && policies.Length > 0) Policies = policies;
    }

    public string[] Policies
    {
        get => Policy[AuthorizationConsts.SilkyAuthorizePrefix.Length..]
            .Split(',', StringSplitOptions.RemoveEmptyEntries);
        internal set => Policy = $"{AuthorizationConsts.SilkyAuthorizePrefix}{string.Join(',', value)}";
    }
}