using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Silky.Http.Core;
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
                        RpcContext.GetContext().SetAttachment(userClaim.Type, userClaim.Value);
                    }

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