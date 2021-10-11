using System;
using Silky.Core.DependencyInjection;
using Silky.Core.Exceptions;
using Silky.Rpc.Diagnostics;
using Silky.Rpc.Runtime.Server;

namespace Silky.Rpc.Runtime
{
    public interface IFallbackDiagnosticListener : IScopedDependency
    {
        long? TracingFallbackBefore(string serviceEntryId, object[] parameters, string messageId,
            FallbackExecType fallbackExecType,
            IFallbackProvider fallbackProvider);

        void TracingFallbackAfter(long? tracingTimestamp, string messageId, string serviceEntryId,
            object result, IFallbackProvider serviceEntryFallbackProvider);

        void TracingFallbackError(long? tracingTimestamp, string messageId, string serviceEntryId,
            StatusCode statusCode,
            Exception ex, IFallbackProvider serviceEntryFallbackProvider);
    }
}