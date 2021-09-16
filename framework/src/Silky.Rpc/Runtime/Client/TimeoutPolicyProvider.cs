using System;
using Polly;
using Silky.Rpc.Runtime.Server;

namespace Silky.Rpc.Runtime.Client
{
    public class TimeoutPolicyProvider : IPolicyProvider
    {
        public IAsyncPolicy Create(ServiceEntry serviceEntry)
        {
            if (serviceEntry.GovernanceOptions.ExecutionTimeoutMillSeconds > 0)
            {
                return Policy.TimeoutAsync(
                    TimeSpan.FromMilliseconds(serviceEntry.GovernanceOptions.ExecutionTimeoutMillSeconds));
            }

            return null;
        }
    }
}