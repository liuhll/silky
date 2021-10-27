using System;
using Polly;
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
                var fallbackPolicy = Policy<object>.Handle<Exception>(ex =>
                    {
                        var isNotNeedFallback = ex is INotNeedFallback;
                        if (isNotNeedFallback)
                        {
                            return false;
                        }

                        return true;
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