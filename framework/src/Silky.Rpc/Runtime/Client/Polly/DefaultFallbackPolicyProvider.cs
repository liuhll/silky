using Polly;
using Silky.Core.Exceptions;
using Silky.Rpc.Runtime.Server;

namespace Silky.Rpc.Runtime.Client
{
    public class DefaultFallbackPolicyProvider : IFallbackPolicyProvider
    {
        private readonly IFallbackInvoker _fallbackInvoker;

        public DefaultFallbackPolicyProvider(IFallbackInvoker fallbackInvoker)
        {
            _fallbackInvoker = fallbackInvoker;
        }

        public IAsyncPolicy<object> Create(ServiceEntry serviceEntry, object[] parameters)
        {
            if (serviceEntry.FallbackMethodExecutor != null && serviceEntry.FallbackProvider != null)
            {
                var fallbackPolicy = Policy<object>.Handle<SilkyException>(ex =>
                    {
                        var isNotNeedFallback = (ex is INotNeedFallback);
                        if (isNotNeedFallback)
                        {
                            return false;
                        }

                        if (ex is BusinessException)
                        {
                            return serviceEntry.FallbackProvider?.ValidWhenBusinessException == true;
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