using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Server.Kestrel.Core.Internal.Http;
using Silky.Core;
using Silky.Core.Extensions;
using Silky.Rpc.Runtime.Server;

namespace Silky.Http.Core
{
    public static class HttpContextExtensions
    {
        public static ServiceEntry GetServiceEntry(this HttpContext context)
        {
            var serviceEntryLocator = EngineContext.Current.Resolve<IServiceEntryLocator>();
            var path = context.Request.Path;
            var method = context.Request.Method.ToEnum<HttpMethod>();
            var serviceEntry = serviceEntryLocator.GetServiceEntryByApi(path, method);
            return serviceEntry;
        }

        public static void SignoutToSwagger(this HttpContext httpContext)
        {
            httpContext.Response.Headers["access-token"] = "invalid_token";
        }
    }
}