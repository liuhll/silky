using System;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Silky.Core;
using Silky.Core.Serialization;
using Silky.Http.Core.Configuration;
using Silky.Rpc.Runtime.Server;

namespace Silky.Http.Core.Handlers
{
    internal abstract class MessageReceivedHandlerBase : IMessageReceivedHandler
    {
        protected readonly ISerializer _serializer;
        protected readonly IOptionsMonitor<GatewayOptions> _gatewayOptionsMonitor;

        internal GatewayOptions GatewayOptions => _gatewayOptionsMonitor.CurrentValue;

        protected MessageReceivedHandlerBase(ISerializer serializer,
            IOptionsMonitor<GatewayOptions> gatewayOptionsMonitor)
        {
            _serializer = serializer;
            _gatewayOptionsMonitor = gatewayOptionsMonitor;
        }

        public virtual ILogger<MessageReceivedHandlerBase> Logger { get; set; }

        public virtual Task Handle([NotNull] ServiceEntry serviceEntry, [NotNull] HttpContext httpContext)
        {
            Check.NotNull(serviceEntry, nameof(serviceEntry));
            Check.NotNull(httpContext, nameof(httpContext));

            var serverCallContext = new HttpContextServerCallContext(httpContext,
                serviceEntry.ServiceEntryDescriptor,
                _serializer, GatewayOptions, Logger);
            httpContext.Features.Set<IServerCallContextFeature>(serverCallContext);
            try
            {
                serverCallContext.Initialize();
                httpContext.RequestAborted = serverCallContext.CancellationToken;
                var handleCallTask = HandleCallAsyncCore(httpContext, serverCallContext, serviceEntry);
                if (handleCallTask.IsCompletedSuccessfully)
                {
                    return serverCallContext.EndCallAsync();
                }
                else
                {
                    return AwaitHandleCall(serverCallContext, handleCallTask);
                }
            }
            catch (Exception ex)
            {
                return serverCallContext.ProcessHandlerErrorAsync(ex);
            }
        }

        public Task Handle(ServiceEntryDescriptor serviceEntryDescriptor, HttpContext httpContext)
        {
            Check.NotNull(serviceEntryDescriptor, nameof(serviceEntryDescriptor));
            Check.NotNull(httpContext, nameof(httpContext));

            var serverCallContext = new HttpContextServerCallContext(httpContext,
                serviceEntryDescriptor,
                _serializer, GatewayOptions, Logger);
            httpContext.Features.Set<IServerCallContextFeature>(serverCallContext);

            try
            {
                serverCallContext.Initialize();
                httpContext.RequestAborted = serverCallContext.CancellationToken;
                var handleCallTask = HandleCallAsyncCore(httpContext, serverCallContext, serviceEntryDescriptor);
                if (handleCallTask.IsCompletedSuccessfully)
                {
                    return serverCallContext.EndCallAsync();
                }
                else
                {
                    return AwaitHandleCall(serverCallContext, handleCallTask);
                }
            }
            catch (Exception ex)
            {
                return serverCallContext.ProcessHandlerErrorAsync(ex);
            }
        }

        static async Task AwaitHandleCall(HttpContextServerCallContext serverCallContext, Task handleCall)
        {
            try
            {
                await handleCall;
                await serverCallContext.EndCallAsync();
            }
            catch (Exception ex)
            {
                await serverCallContext.ProcessHandlerErrorAsync(ex);
            }
        }

        protected abstract Task HandleCallAsyncCore(HttpContext httpContext,
            HttpContextServerCallContext serverCallContext, ServiceEntry serviceEntry);

        protected abstract Task HandleCallAsyncCore(HttpContext httpContext,
            HttpContextServerCallContext serverCallContext,
            ServiceEntryDescriptor serviceEntryDescriptor);
    }
}