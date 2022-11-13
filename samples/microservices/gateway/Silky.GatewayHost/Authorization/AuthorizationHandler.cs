using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Silky.Http.Identity.Authorization.Handlers;
using Silky.Http.Identity.Authorization.Requirements;
using Silky.Rpc.Extensions;
using Silky.Rpc.Runtime.Server;

namespace Silky.GatewayHost.Authorization;

public class AuthorizationHandler : SilkyAuthorizationHandlerBase
{
    protected override async Task<bool> PolicyPipelineAsync(AuthorizationHandlerContext context,
        HttpContext httpContext,
        IAuthorizationRequirement requirement)
    {
        if (requirement is PermissionRequirement permissionRequirement)
        {
            var serviceEntry = httpContext.GetServiceEntry();
            if (serviceEntry?.IsSilkyAppService() == true)
            {
                // todo 
                return true;
            }

            // todo 其他权限配置
            return false;
        }

        return true;
    }
}