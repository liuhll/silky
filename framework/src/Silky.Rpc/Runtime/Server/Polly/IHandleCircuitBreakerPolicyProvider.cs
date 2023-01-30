using System;
using Polly.CircuitBreaker;
using Silky.Core.DependencyInjection;

namespace Silky.Rpc.Runtime.Server
{
    public interface IHandleCircuitBreakerPolicyProvider : ISingletonDependency
    {
        AsyncCircuitBreakerPolicy Create(string serviceEntryId);

        event Action<Exception, TimeSpan> OnBreak;

        event Action OnReset;
    }
}