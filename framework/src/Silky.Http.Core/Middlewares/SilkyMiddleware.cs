using System.Threading.Tasks;
using Silky.Core;
using Silky.Core.Exceptions;
using Silky.Core.Extensions;
using Silky.Rpc.Runtime.Server;
using Microsoft.AspNetCore.Http;
using Silky.Http.Core.Handlers;
using Silky.Rpc;
using Silky.Rpc.MiniProfiler;
using Silky.Rpc.Transport;
using StackExchange.Profiling;
using HttpMethod = Microsoft.AspNetCore.Server.Kestrel.Core.Internal.Http.HttpMethod;

namespace Silky.Http.Core.Middlewares
{
    public class SilkyMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IServiceEntryLocator _serviceEntryLocator;

        public SilkyMiddleware(RequestDelegate next,
            IServiceEntryLocator serviceEntryLocator)
        {
            _next = next;
            _serviceEntryLocator = serviceEntryLocator;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            using var timing = MiniProfiler.Current.StepIf(MiniProfileConstant.RemoteInvoker.Name, 0.1M);
            var path = context.Request.Path;
            var method = context.Request.Method.ToEnum<HttpMethod>();
            var serviceEntry = _serviceEntryLocator.GetServiceEntryByApi(path, method);
            if (serviceEntry != null)
            {
                if (serviceEntry.GovernanceOptions.ProhibitExtranet)
                {
                    throw new FuseProtectionException($"Id为{serviceEntry.Id}的服务条目不允许外网访问");
                }

              
                MiniProfilerPrinter.Print(MiniProfileConstant.Route.Name, MiniProfileConstant.Route.State.FindServiceEntry,
                    $"通过{path}-{method}查找到服务条目{serviceEntry.Id}");
                RpcContext.GetContext().SetAttachment(AttachmentKeys.Path, path.ToString());
                RpcContext.GetContext().SetAttachment(AttachmentKeys.HttpMethod, method.ToString());
                await EngineContext.Current
                    .ResolveNamed<IMessageReceivedHandler>(serviceEntry.ServiceDescriptor.ServiceProtocol.ToString())
                    .Handle(context, serviceEntry);
            }
            else
            {
                MiniProfilerPrinter.Print(MiniProfileConstant.Route.Name, MiniProfileConstant.Route.State.FindServiceEntry,
                    $"通过{path}-{method}没有查找到服务条目",
                    true);
                await _next(context);
            }
        }
        
    }
}