using System;
using System.Collections.Generic;
using Polly;
using Silky.Core.Exceptions;
using Silky.Rpc.Runtime.Server;

namespace Silky.Rpc.Runtime.Client
{
    public class DefaultPolicyBuilder : IPolicyBuilder
    {
        private readonly IFallbackInvoker _fallbackInvoker;
        private readonly ICollection<IPolicyWithResultProvider> _policyWithResultProviders;

        private readonly ICollection<IPolicyProvider> _policyProviders;

        public DefaultPolicyBuilder(IFallbackInvoker fallbackInvoker,
            ICollection<IPolicyProvider> policyProviders,
            ICollection<IPolicyWithResultProvider> policyWithResultProviders)
        {
            _fallbackInvoker = fallbackInvoker;
            _policyProviders = policyProviders;
            _policyWithResultProviders = policyWithResultProviders;
        }

        public event RpcInvokeFallbackHandle OnInvokeFallback;

        public IAsyncPolicy<object> Build(ServiceEntry serviceEntry, object[] parameters)
        {
            IAsyncPolicy<object> policy = Policy.NoOpAsync<object>();

            foreach (var policyProvider in _policyWithResultProviders)
            {
                var policyItem = policyProvider.Create(serviceEntry);
                if (policyItem != null)
                {
                    policy = policy.WrapAsync(policyItem);
                }
            }

            foreach (var policyProvider in _policyProviders)
            {
                var policyItem = policyProvider.Create(serviceEntry);
                if (policyItem != null)
                {
                    policy = policy.WrapAsync(policyItem);
                }
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