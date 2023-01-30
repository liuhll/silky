using System;
using Polly;
using Silky.Core;
using Silky.Core.Exceptions;
using Silky.Rpc.Runtime.Server;

namespace Silky.Rpc.Runtime.Client
{
    public class DefaultInvokeFallbackPolicyProvider : IInvokeFallbackPolicyProvider
    {
        private readonly IFallbackInvoker _fallbackInvoker;
        private readonly IServiceEntryLocator _serviceEntryLocator;

        public DefaultInvokeFallbackPolicyProvider(IFallbackInvoker fallbackInvoker,
            IServiceEntryLocator serviceEntryLocator)
        {
            _fallbackInvoker = fallbackInvoker;
            _serviceEntryLocator = serviceEntryLocator;
        }

        public IAsyncPolicy<object> Create(string serviceEntryId, object[] parameters)
        {
            var serviceEntry = _serviceEntryLocator.GetServiceEntryById(serviceEntryId);
            if (serviceEntry is { FallbackMethodExecutor: { }, FallbackProvider: { } })
            {
                var isRegistered = EngineContext.Current.IsRegistered(serviceEntry.FallbackProvider.Type);
                if (!isRegistered)
                {
                    return null;
                }
                var fallbackPolicy = Policy<object>.Handle<Exception>(ex =>
                    {
                        var isNotNeedFallback = ex is INotNeedFallback;
                        return !isNotNeedFallback;
                    })
                    .FallbackAsync(
                        async (ctx, t) => await _fallbackInvoker.Invoke(serviceEntry, parameters),
                        async (dr, context) =>
                        {
                            if (OnInvokeFallback != null)
                            {
                                await OnInvokeFallback.Invoke(dr, context);
                            }
                        });

                return fallbackPolicy;
            }

            return null;
        }

        public event RpcInvokeFallbackHandle OnInvokeFallback;
    }
}