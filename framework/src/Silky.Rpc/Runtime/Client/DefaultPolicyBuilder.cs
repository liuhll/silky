using System;
using Polly;
using Silky.Core.Exceptions;
using Silky.Rpc.Runtime.Server;

namespace Silky.Rpc.Runtime.Client
{
    public class DefaultPolicyBuilder : IPolicyBuilder
    {
        private readonly IFallbackInvoker _fallbackInvoker;

        public DefaultPolicyBuilder(IFallbackInvoker fallbackInvoker)
        {
            _fallbackInvoker = fallbackInvoker;
        }

        public event RpcInvokeFallbackHandle OnInvokeFallback;

        public IAsyncPolicy<object> Build(ServiceEntry serviceEntry, object[] parameters)
        {
            IAsyncPolicy<object> policy = Policy.NoOpAsync<object>();

            if (serviceEntry.GovernanceOptions.FailoverCount > 0)
            {
                policy.WrapAsync(Policy<object>
                    .Handle<CommunicatonException>()
                    .RetryAsync(serviceEntry.GovernanceOptions.FailoverCount)
                );
            }

            if (serviceEntry.GovernanceOptions.ExecutionTimeoutMillSeconds > 0)
            {
                policy.WrapAsync(Policy.TimeoutAsync(
                    TimeSpan.FromMilliseconds(serviceEntry.GovernanceOptions.ExecutionTimeoutMillSeconds)));
            }

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

                policy = fallbackPolicy.WrapAsync(policy);
            }

            return policy;
        }
    }
}