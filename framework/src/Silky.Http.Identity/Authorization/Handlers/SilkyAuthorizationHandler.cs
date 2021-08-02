using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Silky.Http.Core;
using Silky.Http.Identity.Authorization.Extensions;

namespace Silky.Http.Identity.Authorization.Handlers
{
    public abstract class SilkyAuthorizationHandler : IAuthorizationHandler
    {
        public virtual Task<bool> PipelineAsync(AuthorizationHandlerContext context, DefaultHttpContext httpContext)
        {
            return Task.FromResult(true);
        }
        
        public virtual Task<bool> PolicyPipelineAsync(AuthorizationHandlerContext context, DefaultHttpContext httpContext, IAuthorizationRequirement requirement)
        {
            return Task.FromResult(true);
        }

        public async Task HandleAsync(AuthorizationHandlerContext context)
        {
            var httpContext = context.GetCurrentHttpContext();
            var serviceEntry = httpContext.GetServiceEntry();
            var isAuthenticated = context.User.Identity?.IsAuthenticated ?? false;
            if (serviceEntry != null && isAuthenticated)
            {
                await AuthorizeHandleAsync(context, httpContext);
            }
            else
            {
                var pendingRequirements = context.PendingRequirements;
                foreach (var requirement in pendingRequirements)
                {
                    // 验证策略管道
                    var policyPipeline = await PolicyPipelineAsync(context, httpContext, requirement);
                    if (policyPipeline) context.Succeed(requirement);
                }
            }

        }

        private async Task AuthorizeHandleAsync(AuthorizationHandlerContext context, DefaultHttpContext httpContext)

        {
            // 获取所有未成功验证的需求
            var pendingRequirements = context.PendingRequirements;

            var pipeline = await PipelineAsync(context, httpContext);
            if (pipeline)
            {
                // 通过授权验证
                foreach (var requirement in pendingRequirements)
                {
                    // 验证策略管道
                    var policyPipeline = await PolicyPipelineAsync(context, httpContext, requirement);
                    if (policyPipeline) context.Succeed(requirement);
                }
            }
            else context.Fail();
        }
    }
}