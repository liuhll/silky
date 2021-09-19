using System;
using Polly.CircuitBreaker;
using Silky.Core.DependencyInjection;
using Silky.Rpc.Transport.Messages;

namespace Silky.Rpc.Runtime.Server
{
    public interface IHandleCircuitBreakerPolicyProvider : IScopedDependency
    {
        AsyncCircuitBreakerPolicy Create(RemoteInvokeMessage message);

        event Action<Exception, TimeSpan> OnBreak;

        event Action OnReset;
    }
}