using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Silky.Core;
using Silky.Core.Rpc;
using Silky.Http.Core;
using Silky.Http.Identity.Extensions;
using Silky.Rpc.Transport;

namespace Silky.Http.Identity.Authentication.Middlewares
{
    public class SilkyAuthenticationMiddleware
    {
        private readonly RequestDelegate _next;

        public SilkyAuthenticationMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var serviceEntry = context.GetServiceEntry();
            if (serviceEntry != null && !serviceEntry.GovernanceOptions.IsAllowAnonymous)
            {
                if (context.User.Identity?.IsAuthenticated == true)
                {
                    foreach (var userClaim in context.User.Claims)
                    {
                        RpcContext.Context.SetAttachment(userClaim.Type, userClaim.Value);
                    }

                    await _next(context);
                }
            }
            else if (serviceEntry != null && serviceEntry.IsSilkyAppService())
            {
                var silkyAppServiceUseAuth =
                    EngineContext.Current.Configuration.GetValue<bool?>("dashboard:useAuth") ?? false;
                if (!silkyAppServiceUseAuth)
                {
                    await _next(context);
                }
            }
            else
            {
                await _next(context);
            }
        }
    }
}