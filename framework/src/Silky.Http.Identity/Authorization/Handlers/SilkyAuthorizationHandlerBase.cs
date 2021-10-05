using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Silky.Http.Identity.Authorization.Extensions;

namespace Silky.Http.Identity.Authorization.Handlers
{
    public abstract class SilkyAuthorizationHandlerBase : IAuthorizationHandler
    {
        public virtual Task<bool> PipelineAsync(AuthorizationHandlerContext context, DefaultHttpContext httpContext)
        {
            return Task.FromResult(true);
        }

        public async Task HandleAsync(AuthorizationHandlerContext context)
        {
            var isAuthenticated = context.User.Identity?.IsAuthenticated;
            if (isAuthenticated == true)
            {
                await AuthorizeHandleAsync(context);
            }
        }

        private async Task AuthorizeHandleAsync(AuthorizationHandlerContext context)
        {
            var httpContext = context.GetCurrentHttpContext();
            var pipeline = await PipelineAsync(context, httpContext);
            if (!pipeline)
            {
                context.Fail();
            }
        }
    }
}