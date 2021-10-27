using System.Collections.Generic;
using Polly;

namespace Silky.Rpc.Runtime.Client
{
    internal sealed class DefaultInvokePolicyBuilder : IInvokePolicyBuilder
    {
        private readonly ICollection<IPolicyWithResultProvider> _policyWithResultProviders;

        private readonly ICollection<IInvokePolicyProvider> _policyProviders;

        private readonly ICollection<IInvokeCircuitBreakerPolicyProvider> _circuitBreakerPolicyProviders;

        public DefaultInvokePolicyBuilder(
            ICollection<IInvokePolicyProvider> policyProviders,
            ICollection<IPolicyWithResultProvider> policyWithResultProviders,
            ICollection<IInvokeCircuitBreakerPolicyProvider> circuitBreakerPolicyProviders)
        {
            _policyProviders = policyProviders;
            _policyWithResultProviders = policyWithResultProviders;
            _circuitBreakerPolicyProviders = circuitBreakerPolicyProviders;
        }


        public IAsyncPolicy<object> Build(string serviceEntryId, object[] parameters)
        {
            IAsyncPolicy<object> policy = Policy.NoOpAsync<object>();

            foreach (var policyProvider in _policyWithResultProviders)
            {
                var policyItem = policyProvider.Create(serviceEntryId, parameters);
                if (policyItem != null)
                {
                    policy = policy.WrapAsync(policyItem);
                }
            }

            foreach (var policyProvider in _policyProviders)
            {
                var policyItem = policyProvider.Create(serviceEntryId, parameters);
                if (policyItem != null)
                {
                    policy = policy.WrapAsync(policyItem);
                }
            }

            foreach (var circuitBreakerPolicyProvider in _circuitBreakerPolicyProviders)
            {
                var policyItem = circuitBreakerPolicyProvider.Create(serviceEntryId, parameters);
                policy = policy.WrapAsync(policyItem);
            }

            return policy;
        }
    }
}