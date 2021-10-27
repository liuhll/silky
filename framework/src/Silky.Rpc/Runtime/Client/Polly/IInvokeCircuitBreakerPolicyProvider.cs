using System;
using Polly.CircuitBreaker;
using Silky.Core.DependencyInjection;

namespace Silky.Rpc.Runtime.Client
{
    public interface IInvokeCircuitBreakerPolicyProvider : IScopedDependency
    {
        AsyncCircuitBreakerPolicy Create(string serviceEntryId, object[] parameters);

        event Action<Exception, TimeSpan> OnBreak;

        event Action OnReset;
    }
}