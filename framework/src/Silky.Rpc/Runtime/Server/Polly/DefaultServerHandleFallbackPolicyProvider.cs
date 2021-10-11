using System;
using Polly;
using Silky.Core.Exceptions;
using Silky.Rpc.Transport.Messages;

namespace Silky.Rpc.Runtime.Server
{
    public class DefaultServerHandleFallbackPolicyProvider : IServerHandleFallbackPolicyProvider
    {
        private readonly IServerFallbackHandler _serverFallbackHandler;

        public DefaultServerHandleFallbackPolicyProvider(IServerFallbackHandler serverFallbackHandler)
        {
            _serverFallbackHandler = serverFallbackHandler;
        }

        public IAsyncPolicy<RemoteResultMessage> Create(RemoteInvokeMessage message)
        {
            var fallbackPolicy = Policy<RemoteResultMessage>
                .Handle<Exception>(exception => !(exception is INotNeedFallback)).FallbackAsync(
                    async (ctx, t) => await _serverFallbackHandler.Handle(message, ctx, t),
                    async (dr, context) =>
                    {
                        if (OnHandleFallback != null)
                        {
                            await OnHandleFallback.Invoke(dr, context);
                        }
                    });
            return fallbackPolicy;
        }

        public event RpcHandleFallbackHandle OnHandleFallback;
    }
}