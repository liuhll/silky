using System.Collections.Generic;
using Polly;
using Silky.Rpc.Transport.Messages;

namespace Silky.Rpc.Runtime.Server
{
    public class DefaultHandlePolicyBuilder : IHandlePolicyBuilder
    {
        private readonly ICollection<IHandlePolicyProvider> _handlePolicyProviders;
        private readonly ICollection<IHandlePolicyWithResultProvider> _handlePolicyWithResultProviders;
        private readonly ICollection<IHandleCircuitBreakerPolicyProvider> _circuitBreakerPolicyProviders;

        public DefaultHandlePolicyBuilder(ICollection<IHandlePolicyProvider> handlePolicyProviders,
            ICollection<IHandlePolicyWithResultProvider> handlePolicyWithResultProviders,
            ICollection<IHandleCircuitBreakerPolicyProvider> circuitBreakerPolicyProviders)
        {
            _handlePolicyProviders = handlePolicyProviders;
            _handlePolicyWithResultProviders = handlePolicyWithResultProviders;
            _circuitBreakerPolicyProviders = circuitBreakerPolicyProviders;
        }

        public IAsyncPolicy<RemoteResultMessage> Build(RemoteInvokeMessage remoteInvokeMessage)
        {
            IAsyncPolicy<RemoteResultMessage> policy = Policy.NoOpAsync<RemoteResultMessage>();

            foreach (var handlePolicyProvider in _handlePolicyProviders)
            {
                var policyItem = handlePolicyProvider.Create(remoteInvokeMessage);
                if (policyItem != null)
                {
                    policy = policy.WrapAsync(policyItem);
                }
            }

            foreach (var handlePolicyWithResultProviders in _handlePolicyWithResultProviders)
            {
                var policyItem = handlePolicyWithResultProviders.Create(remoteInvokeMessage);
                if (policyItem != null)
                {
                    policy = policy.WrapAsync(policyItem);
                }
            }

            foreach (var circuitBreakerPolicyProvider in _circuitBreakerPolicyProviders)
            {
                var policyItem = circuitBreakerPolicyProvider.Create(remoteInvokeMessage);
                policy = policy.WrapAsync(policyItem);
            }

            return policy;
        }
    }
}