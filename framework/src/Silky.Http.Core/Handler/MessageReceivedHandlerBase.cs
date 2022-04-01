using System;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Silky.Core;
using Silky.Core.Serialization;
using Silky.Rpc.Runtime.Server;

namespace Silky.Http.Core.Handlers
{
    internal abstract class MessageReceivedHandlerBase : IMessageReceivedHandler
    {
        protected readonly ISerializer _serializer;

        protected MessageReceivedHandlerBase(ISerializer serializer)
        {
            _serializer = serializer;
        }

        public virtual ILogger<MessageReceivedHandlerBase> Logger { get; set; }

        public virtual Task Handle([NotNull] ServiceEntry serviceEntry, [NotNull] HttpContext httpContext)
        {
            Check.NotNull(serviceEntry, nameof(serviceEntry));
            Check.NotNull(httpContext, nameof(httpContext));

            var serverCallContext = new HttpContextServerCallContext(httpContext, serviceEntry, _serializer, Logger);
            httpContext.Features.Set<IServerCallContextFeature>(serverCallContext);

            try
            {
                serverCallContext.Initialize();

                var handleCallTask = HandleCallAsyncCore(httpContext, serverCallContext);
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
            HttpContextServerCallContext serverCallContext);
    }
}