using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Silky.Core;
using Silky.Core.Rpc;
using Silky.Http.Core;
using Silky.Http.Identity.Authorization.Extensions;
using Silky.Http.Identity.Extensions;

namespace Silky.Http.Identity.Authorization.Handlers
{
    public abstract class SilkyAuthorizationHandler : IAuthorizationHandler
    {
        public virtual Task<bool> PipelineAsync(AuthorizationHandlerContext context, DefaultHttpContext httpContext)
        {
            return Task.FromResult(true);
        }

        public virtual Task<bool> PolicyPipelineAsync(AuthorizationHandlerContext context,
            DefaultHttpContext httpContext, IAuthorizationRequirement requirement)
        {
            return Task.FromResult(true);
        }

        public async Task HandleAsync(AuthorizationHandlerContext context)
        {
            async Task HttpContextPipelineAuthorize(DefaultHttpContext httpContext)
            {
                var pendingRequirements = context.PendingRequirements;
                foreach (var requirement in pendingRequirements)
                {
                    // 验证策略管道
                    var policyPipeline = await PolicyPipelineAsync(context, httpContext, requirement);
                    if (policyPipeline) context.Succeed(requirement);
                }
            }

            var httpContext = context.GetCurrentHttpContext();
            var serviceEntry = httpContext.GetServiceEntry();
            if (serviceEntry != null)
            {
                var isAuthenticated = context.User.Identity?.IsAuthenticated ?? false;
                if (isAuthenticated)
                {
                    foreach (var userClaim in context.User.Claims)
                    {
                        RpcContext.Context.SetAttachment(userClaim.Type, userClaim.Value);
                    }
            
                    await AuthorizeHandleAsync(context, httpContext);
                }
                else if (!serviceEntry.GovernanceOptions.IsAllowAnonymous)
                {
                    if (serviceEntry.IsSilkyAppService())
                    {
                        var silkyAppServiceUseAuth =
                            EngineContext.Current.Configuration.GetValue<bool?>("dashboard:useAuth") ?? false;
                        if (silkyAppServiceUseAuth)
                        {
                            context.Fail();
                        }
                        else
                        {
                            await HttpContextPipelineAuthorize(httpContext);
                        }
                    }
                    else
                    {
                        context.Fail();
                    }
                }
                else
                {
                    await HttpContextPipelineAuthorize(httpContext);
                }
            }
            else
            {
                await HttpContextPipelineAuthorize(httpContext);
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