using System.Collections.Concurrent;
using System.Collections.Generic;
using Polly;

namespace Silky.Rpc.Runtime.Client
{
    internal sealed class DefaultInvokePolicyBuilder : IInvokePolicyBuilder
    {
        private readonly ICollection<IPolicyWithResultProvider> _policyWithResultProviders;

        private readonly ICollection<IInvokePolicyProvider> _policyProviders;

        private readonly ICollection<IInvokeCircuitBreakerPolicyProvider> _circuitBreakerPolicyProviders;
        private readonly ICollection<IInvokeFallbackPolicyProvider> _invokeFallbackPolicyProviders;

        private readonly ConcurrentDictionary<string, IAsyncPolicy<object?>> _policyCaches = new();

        public DefaultInvokePolicyBuilder(
            ICollection<IInvokePolicyProvider> policyProviders,
            ICollection<IPolicyWithResultProvider> policyWithResultProviders,
            ICollection<IInvokeCircuitBreakerPolicyProvider> circuitBreakerPolicyProviders,
            ICollection<IInvokeFallbackPolicyProvider> invokeFallbackPolicyProviders)
        {
            _policyProviders = policyProviders;
            _policyWithResultProviders = policyWithResultProviders;
            _circuitBreakerPolicyProviders = circuitBreakerPolicyProviders;
            _invokeFallbackPolicyProviders = invokeFallbackPolicyProviders;
        }


        public IAsyncPolicy<object?> Build(string serviceEntryId)
        {
            if (_policyCaches.TryGetValue(serviceEntryId, out var policy))
            {
                return policy;
            }

            policy = Policy.NoOpAsync<object>();

            foreach (var policyProvider in _policyWithResultProviders)
            {
                var policyItem = policyProvider.Create(serviceEntryId);
                if (policyItem != null)
                {
                    policy = policy.WrapAsync(policyItem);
                }
            }

            foreach (var policyProvider in _policyProviders)
            {
                var policyItem = policyProvider.Create(serviceEntryId);
                if (policyItem != null)
                {
                    policy = policy.WrapAsync(policyItem);
                }
            }

            foreach (var circuitBreakerPolicyProvider in _circuitBreakerPolicyProviders)
            {
                var policyItem = circuitBreakerPolicyProvider.Create(serviceEntryId);
                if (policyItem != null)
                {
                    policy = policy.WrapAsync(policyItem);
                }
            }

            _policyCaches.TryAdd(serviceEntryId, policy);
            return policy;
        }

        public IAsyncPolicy<object?> Build(string serviceEntryId, object[] parameters)
        {
            var policy = Build(serviceEntryId);
            foreach (var invokeFallbackPolicyProvider in _invokeFallbackPolicyProviders)
            {
                var policyItem = invokeFallbackPolicyProvider.Create(serviceEntryId, parameters);
                if (policyItem != null)
                {
                    policy = policy.WrapAsync(policyItem);
                }
            }

            return policy;
        }
    }
}