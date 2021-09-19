using System;
using Polly;
using Polly.Timeout;
using Silky.Rpc.Runtime.Server;

namespace Silky.Rpc.Runtime.Client
{
    public class DefaultTimeoutInvokePolicyProvider : IInvokePolicyProvider
    {
        public IAsyncPolicy Create(ServiceEntry serviceEntry, object[] parameters)
        {
            if (serviceEntry.GovernanceOptions.TimeoutMillSeconds > 0)
            {
                return Policy.TimeoutAsync(
                    TimeSpan.FromMilliseconds(serviceEntry.GovernanceOptions.TimeoutMillSeconds),
                    TimeoutStrategy.Pessimistic);
            }

            return null;
        }
    }
}