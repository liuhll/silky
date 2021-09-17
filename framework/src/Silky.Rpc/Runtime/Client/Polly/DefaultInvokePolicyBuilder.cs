using System.Collections.Generic;
using Polly;
using Silky.Rpc.Runtime.Server;

namespace Silky.Rpc.Runtime.Client
{
    public class DefaultInvokePolicyBuilder : IInvokePolicyBuilder
    {
        private readonly ICollection<IPolicyWithResultProvider> _policyWithResultProviders;

        private readonly ICollection<IPolicyProvider> _policyProviders;

        public DefaultInvokePolicyBuilder(
            ICollection<IPolicyProvider> policyProviders,
            ICollection<IPolicyWithResultProvider> policyWithResultProviders)
        {
            _policyProviders = policyProviders;
            _policyWithResultProviders = policyWithResultProviders;
        }


        public IAsyncPolicy<object> Build(ServiceEntry serviceEntry, object[] parameters)
        {
            IAsyncPolicy<object> policy = Policy.NoOpAsync<object>();

            foreach (var policyProvider in _policyWithResultProviders)
            {
                var policyItem = policyProvider.Create(serviceEntry, parameters);
                if (policyItem != null)
                {
                    policy = policy.WrapAsync(policyItem);
                }
            }

            foreach (var policyProvider in _policyProviders)
            {
                var policyItem = policyProvider.Create(serviceEntry, parameters);
                if (policyItem != null)
                {
                    policy = policy.WrapAsync(policyItem);
                }
            }


            return policy;
        }
    }
}