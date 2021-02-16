using System.Threading.Tasks;
using Lms.Core;
using Lms.Core.Exceptions;
using Lms.Core.Extensions;
using Lms.Rpc.Runtime.Server;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Server.Kestrel.Core.Internal.Http;

namespace Lms.HttpServer.Middlewares
{
    public class LmsMiddleware
    {
        private readonly RequestDelegate _next;

        public LmsMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var path = context.Request.Path;
            var method = context.Request.Method.ToEnum<HttpMethod>();
            var serviceEntryLocator = EngineContext.Current.Resolve<IServiceEntryLocator>();
            var serviceEntry = serviceEntryLocator.GetServiceEntryByApi(path, method);
            if (serviceEntry == null)
            {
                await _next(context);
            }
            else
            {
                await EngineContext.Current.Resolve<HttpMessageReceivedHandler>().Handle(context, serviceEntry);
            }
        }
    }
    
}