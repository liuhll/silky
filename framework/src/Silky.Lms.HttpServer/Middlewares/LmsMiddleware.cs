using System.Threading.Tasks;
using Silky.Lms.Core;
using Silky.Lms.Core.Exceptions;
using Silky.Lms.Core.Extensions;
using Silky.Lms.Rpc.Routing;
using Silky.Lms.Rpc.Runtime.Server;
using Silky.Lms.Rpc.Utils;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Server.Kestrel.Core.Internal.Http;
using Silky.Lms.HttpServer.Handlers;

namespace Silky.Lms.HttpServer.Middlewares
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
                if (serviceEntry.GovernanceOptions.ProhibitExtranet)
                {
                    throw new FuseProtectionException($"Id为{serviceEntry.Id}的服务条目不允许外网访问");
                }
                EngineContext.Current.PrintToMiniProfiler("ServiceEntry","Find",$"通过{path}-{method}查找到服务条目");
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