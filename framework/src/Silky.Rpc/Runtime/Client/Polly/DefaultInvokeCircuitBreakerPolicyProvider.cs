using System;
using Polly;
using Polly.CircuitBreaker;
using Silky.Core.Exceptions;
using Silky.Rpc.Runtime.Server;

namespace Silky.Rpc.Runtime.Client
{
    public class DefaultInvokeCircuitBreakerPolicyProvider : IInvokeCircuitBreakerPolicyProvider
    {
        public AsyncCircuitBreakerPolicy Create(ServiceEntry serviceEntry, object[] parameters)
        {
            if (serviceEntry.GovernanceOptions.EnableCircuitBreaker)
            {
                var policy = Policy
                    .Handle<Exception>(ex => !ex.IsFriendlyException())
                    .CircuitBreakerAsync(
                        exceptionsAllowedBeforeBreaking: serviceEntry.GovernanceOptions.ExceptionsAllowedBeforeBreaking,
                        durationOfBreak: TimeSpan.FromSeconds(serviceEntry.GovernanceOptions.BreakerSeconds),
                        (ex, timespan) =>
                        {
                            if (OnBreak != null)
                            {
                                OnBreak.Invoke(ex, timespan);
                            }
                        },
                        () =>
                        {
                            if (OnReset != null)
                            {
                                OnReset.Invoke();
                            }
                        }
                    );
                return policy;
            }

            return null;
        }

        public event Action<Exception, TimeSpan> OnBreak;

        public event Action OnReset;
    }
}