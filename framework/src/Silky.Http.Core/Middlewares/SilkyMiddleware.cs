using System.Threading.Tasks;
using Silky.Core;
using Silky.Core.Exceptions;
using Silky.Core.Extensions;
using Silky.Rpc.Runtime.Server;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Silky.Core.Rpc;
using Silky.Http.Core.Handlers;
using Silky.Rpc.Extensions;
using Silky.Rpc.MiniProfiler;
using HttpMethod = Microsoft.AspNetCore.Server.Kestrel.Core.Internal.Http.HttpMethod;

namespace Silky.Http.Core.Middlewares
{
    public class SilkyMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IServiceEntryLocator _serviceEntryLocator;
        private readonly ILogger<SilkyMiddleware> _logger;

        public SilkyMiddleware(RequestDelegate next,
            IServiceEntryLocator serviceEntryLocator, 
            ILogger<SilkyMiddleware> logger)
        {
            _next = next;
            _serviceEntryLocator = serviceEntryLocator;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var path = context.Request.Path;
            var method = context.Request.Method.ToEnum<HttpMethod>();
            var serviceEntry = _serviceEntryLocator.GetServiceEntryByApi(path, method);
            if (serviceEntry != null)
            {
                if (serviceEntry.GovernanceOptions.ProhibitExtranet)
                {
                    throw new FuseProtectionException(
                        $"The ServiceEntry whose Id is {serviceEntry.Id} is not allowed to be accessed from the external network");
                }

                _logger.LogWithMiniProfiler(MiniProfileConstant.Route.Name,
                    MiniProfileConstant.Route.State.FindServiceEntry,
                    $"Find the ServiceEntry {serviceEntry.Id} through {path}-{method}");
                RpcContext.Context.SetAttachment(AttachmentKeys.Path, path.ToString());
                RpcContext.Context.SetAttachment(AttachmentKeys.HttpMethod, method.ToString());
                await EngineContext.Current
                    .ResolveNamed<IMessageReceivedHandler>(HttpMessageType.Outer.ToString())
                    .Handle(serviceEntry);
            }
            else
            {
                // todo Consider supporting RPC communication via http protocol

                _logger.LogWithMiniProfiler(MiniProfileConstant.Route.Name,
                    MiniProfileConstant.Route.State.FindServiceEntry,
                    $"No ServiceEntry was found through {path}-{method}",
                    true);
                await _next(context);
            }
        }
    }
}