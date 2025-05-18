using System.Collections.Concurrent;
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
        private readonly ICollection<IServerHandleFallbackPolicyProvider> _serverHandleFallbackPolicyProviders;
        
        private readonly ConcurrentDictionary<string, IAsyncPolicy<RemoteResultMessage>> _policyCaches = new(); 

        public DefaultHandlePolicyBuilder(ICollection<IHandlePolicyProvider> handlePolicyProviders,
            ICollection<IHandlePolicyWithResultProvider> handlePolicyWithResultProviders,
            ICollection<IHandleCircuitBreakerPolicyProvider> circuitBreakerPolicyProviders,
            ICollection<IServerHandleFallbackPolicyProvider> serverHandleFallbackPolicyProviders)
        {
            _handlePolicyProviders = handlePolicyProviders;
            _handlePolicyWithResultProviders = handlePolicyWithResultProviders;
            _circuitBreakerPolicyProviders = circuitBreakerPolicyProviders;
            _serverHandleFallbackPolicyProviders = serverHandleFallbackPolicyProviders;
        }

        public IAsyncPolicy<RemoteResultMessage> Build(RemoteInvokeMessage message)
        {
            var normPolicy = BuildNormPolicy(message.ServiceEntryId);
            foreach (var serverHandleFallbackPolicyProvider in _serverHandleFallbackPolicyProviders)
            {
                var policyItem = serverHandleFallbackPolicyProvider.Create(message);
                normPolicy = normPolicy.WrapAsync(policyItem);
            }

            return normPolicy;
        }

        private IAsyncPolicy<RemoteResultMessage> BuildNormPolicy(string serviceEntryId)
        {
            return _policyCaches.GetOrAdd(serviceEntryId, id =>
            {
                IAsyncPolicy<RemoteResultMessage> policy = Policy.NoOpAsync<RemoteResultMessage>();
                
                foreach (var handlePolicyProvider in _handlePolicyProviders)
                {
                    var policyItem = handlePolicyProvider.Create(serviceEntryId);
                    if (policyItem != null)
                    {
                        policy = policy.WrapAsync(policyItem);
                    }
                }
                
                foreach (var handlePolicyWithResultProvider in _handlePolicyWithResultProviders)
                {
                    var policyItem = handlePolicyWithResultProvider.Create(serviceEntryId);
                    if (policyItem != null)
                    {
                        policy = policy.WrapAsync(policyItem);
                    }
                }
                
                foreach (var circuitBreakerPolicyProvider in _circuitBreakerPolicyProviders)
                {
                    var policyItem = circuitBreakerPolicyProvider.Create(serviceEntryId);
                    policy = policy.WrapAsync(policyItem);
                }
                
                _policyCaches.TryAdd(serviceEntryId, policy);
                return policy;
            });
        }
    }
}