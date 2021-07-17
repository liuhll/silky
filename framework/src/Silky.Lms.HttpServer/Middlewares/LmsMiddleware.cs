using System;
using System.Threading.Tasks;
using Silky.Lms.Core;
using Silky.Lms.Core.Exceptions;
using Silky.Lms.Core.Extensions;
using Silky.Lms.Rpc.Runtime.Server;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Server.Kestrel.Core.Internal.Http;
using Silky.Lms.HttpServer.Handlers;
using StackExchange.Profiling;

namespace Silky.Lms.HttpServer.Middlewares
{
    public class LmsMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IServiceEntryLocator _serviceEntryLocator;
        private readonly IMiniProfiler _miniProfiler;
        public LmsMiddleware(RequestDelegate next,
            IServiceEntryLocator serviceEntryLocator,
            IMiniProfiler miniProfiler)
        {
            _next = next;
            _serviceEntryLocator = serviceEntryLocator;
            _miniProfiler = miniProfiler;
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
                _miniProfiler.Print(MiniProfileConstant.Route.Name, MiniProfileConstant.Route.State.FindServiceEntry,
                    $"通过{path}-{method}查找到服务条目{serviceEntry.Id}");
                await EngineContext.Current
                    .ResolveNamed<IMessageReceivedHandler>(serviceEntry.ServiceDescriptor.ServiceProtocol.ToString())
                    .Handle(context, serviceEntry);
            }
            else
            {
                _miniProfiler.Print(MiniProfileConstant.Route.Name, MiniProfileConstant.Route.State.FindServiceEntry, $"通过{path}-{method}没有查找到服务条目",
                    true);
                await _next(context);
            }
           
        }
    }
}