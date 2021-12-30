using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Silky.Core;
using Silky.Core.Rpc;
using Silky.Http.Identity.Authorization.Extensions;
using Silky.Rpc.Extensions;
using Silky.Rpc.Security;

namespace Silky.Http.Identity.Authorization.Handlers
{
    public abstract class SilkyAuthorizationHandlerBase : IAuthorizationHandler
    {
        protected virtual Task<bool> PipelineAsync(AuthorizationHandlerContext context, HttpContext httpContext)
        {
            return Task.FromResult(true);
        }

        public async Task HandleAsync(AuthorizationHandlerContext context)
        {
            var isAuthenticated = context.User.Identity?.IsAuthenticated;
            if (isAuthenticated == true)
            {
                var httpContext = context.GetCurrentHttpContext();
                httpContext.SetUserClaims();
                httpContext.SetHttpHandleAddressInfo();
                var currentRpcToken = EngineContext.Current.Resolve<ICurrentRpcToken>();
                currentRpcToken.SetRpcToken();
                await AuthorizeHandleAsync(context, httpContext);
            }
        }

        private async Task AuthorizeHandleAsync(AuthorizationHandlerContext context, HttpContext httpContext)
        {
            var pipeline = await PipelineAsync(context, httpContext);
            if (!pipeline)
            {
                context.Fail();
            }
        }
    }
}