using System;
using Polly;
using Polly.CircuitBreaker;
using Silky.Core.Exceptions;
using Silky.Rpc.Runtime.Server;

namespace Silky.Rpc.Runtime.Client
{
    public class DefaultInvokeCircuitBreakerPolicyProvider : IInvokeCircuitBreakerPolicyProvider
    {
        private readonly IServerManager _serverManager;

        public DefaultInvokeCircuitBreakerPolicyProvider(IServerManager serverManager)
        {
            _serverManager = serverManager;
        }

        public AsyncCircuitBreakerPolicy Create(string serviceEntryId)
        {
            var serviceEntryDescriptor = _serverManager.GetServiceEntryDescriptor(serviceEntryId);
            if (serviceEntryDescriptor?.GovernanceOptions.EnableCircuitBreaker == true)
            {
                var policy = Policy
                    .Handle<Exception>(ex =>
                    {
                        var isFriendlyException = ex.IsFriendlyException();
                        return !isFriendlyException;
                    })
                    .CircuitBreakerAsync(
                        exceptionsAllowedBeforeBreaking: serviceEntryDescriptor.GovernanceOptions
                            .ExceptionsAllowedBeforeBreaking,
                        durationOfBreak: TimeSpan.FromSeconds(serviceEntryDescriptor.GovernanceOptions.BreakerSeconds),
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