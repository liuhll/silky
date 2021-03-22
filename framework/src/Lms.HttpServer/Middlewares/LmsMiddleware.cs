using System.Threading.Tasks;
using Lms.Core;
using Lms.Core.Extensions;
using Lms.HttpServer.Handlers;
using Lms.Rpc.Routing;
using Lms.Rpc.Runtime.Server;
using Lms.Rpc.Utils;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Server.Kestrel.Core.Internal.Http;

namespace Lms.HttpServer.Middlewares
{
    public class LmsMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IServiceEntryLocator _serviceEntryLocator;
        public LmsMiddleware(RequestDelegate next,
            IServiceEntryLocator serviceEntryLocator)
        {
            _next = next;
            _serviceEntryLocator = serviceEntryLocator;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var path = context.Request.Path;
            var method = context.Request.Method.ToEnum<HttpMethod>();


            var serviceEntry = _serviceEntryLocator.GetServiceEntryByApi(path, method);
            if (serviceEntry != null)
            {
                await EngineContext.Current
                    .ResolveNamed<IMessageReceivedHandler>(serviceEntry.ServiceDescriptor.ServiceProtocol.ToString())
                    .Handle(context, serviceEntry);
            }
            else
            {
                await _next(context);
            }
        }
    }
}