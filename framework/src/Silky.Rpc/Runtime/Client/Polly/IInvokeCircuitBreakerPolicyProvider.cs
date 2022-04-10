using System;
using Polly.CircuitBreaker;
using Silky.Core.DependencyInjection;

namespace Silky.Rpc.Runtime.Client
{
    public interface IInvokeCircuitBreakerPolicyProvider : ISingletonDependency
    {
        AsyncCircuitBreakerPolicy Create(string serviceEntryId);

        event Action<Exception, TimeSpan> OnBreak;

        event Action OnReset;
    }
}