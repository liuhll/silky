using System;
using Polly;
using Polly.Timeout;
using Silky.Rpc.Runtime.Server;

namespace Silky.Rpc.Runtime.Client
{
    public class DefaultTimeoutInvokePolicyProvider : IInvokePolicyProvider
    {
        private readonly IServerManager _serverManager;

        public DefaultTimeoutInvokePolicyProvider(IServerManager serverManager)
        {
            _serverManager = serverManager;
        }

        public IAsyncPolicy Create(string serviceEntryId)
        {
            var serviceEntryDescriptor = _serverManager.GetServiceEntryDescriptor(serviceEntryId);
            if (serviceEntryDescriptor?.GovernanceOptions.TimeoutMillSeconds > 0)
            {
                return Policy.TimeoutAsync(
                    TimeSpan.FromMilliseconds(serviceEntryDescriptor.GovernanceOptions.TimeoutMillSeconds),
                    TimeoutStrategy.Pessimistic);
            }

            return null;
        }
    }
}