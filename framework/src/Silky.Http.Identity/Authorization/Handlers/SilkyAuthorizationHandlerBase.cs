using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Silky.Core;
using Silky.Http.Identity.Authorization.Extensions;
using Silky.Rpc.Extensions;
using Silky.Rpc.Security;

namespace Silky.Http.Identity.Authorization.Handlers
{
    public abstract class SilkyAuthorizationHandlerBase : IAuthorizationHandler
    {
        /// <summary>
        /// 验证管道
        /// </summary>
        /// <param name="context"></param>
        /// <param name="httpContext"></param>
        /// <returns></returns>
        protected virtual Task<bool> PipelineAsync(AuthorizationHandlerContext context, HttpContext httpContext)
        {
            return Task.FromResult(true);
        }

        /// <summary>
        /// 策略验证管道
        /// </summary>
        /// <param name="context"></param>
        /// <param name="httpContext"></param>
        /// <param name="requirement"></param>
        /// <returns></returns>
        protected virtual Task<bool> PolicyPipelineAsync(AuthorizationHandlerContext context, HttpContext httpContext,
            IAuthorizationRequirement requirement)
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
            var pendingRequirements = context.PendingRequirements;

            var pipeline = await PipelineAsync(context, httpContext);
            if (pipeline)
            {
                foreach (var requirement in pendingRequirements)
                {
                    var policyPipeline = await PolicyPipelineAsync(context, httpContext, requirement);
                    if (policyPipeline) context.Succeed(requirement);
                }
            }
            else
            {
                context.Fail();
            }
        }
    }
}