using System;
using Polly.CircuitBreaker;
using Silky.Core.DependencyInjection;
using Silky.Rpc.Runtime.Server;

namespace Silky.Rpc.Runtime.Client
{
    public interface IInvokeCircuitBreakerPolicyProvider : IScopedDependency
    {
        AsyncCircuitBreakerPolicy Create(ServiceEntry serviceEntry, object[] parameters);

        event Action<Exception, TimeSpan> OnBreak;

        event Action OnReset;
    }
}