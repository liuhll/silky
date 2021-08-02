using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;

namespace Silky.Http.Identity.Authorization.Handlers
{
    public sealed class DefaultSilkyAuthorizationHandler : SilkyAuthorizationHandler
    {
        public override Task<bool> PipelineAsync(AuthorizationHandlerContext context, DefaultHttpContext httpContext)
        {
            return base.PipelineAsync(context, httpContext);
        }
    }
}