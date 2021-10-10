using System;
using System.Diagnostics;
using Silky.Core.Exceptions;
using Silky.Rpc.Diagnostics;
using Silky.Rpc.Runtime.Server;

namespace Silky.Rpc.Runtime
{
    public class DefaultFallbackDiagnosticListener : IFallbackDiagnosticListener
    {
        private static readonly DiagnosticListener s_diagnosticListener =
            new(RpcDiagnosticListenerNames.DiagnosticFallbackListenerName);

        public long? TracingFallbackBefore(string serviceEntryId, object[] parameters, string messageId,
            FallbackExecType fallbackExecType,
            IFallbackProvider fallbackProvider)
        {
            if (s_diagnosticListener.IsEnabled(RpcDiagnosticListenerNames.RpcFallbackBegin))
            {
                var eventData = new FallbackEventData()
                {
                    ServiceEntryId = serviceEntryId,
                    Parameters = parameters,
                    MessageId = messageId,
                    FallbackExecType = fallbackExecType,
                    FallbackProvider = fallbackProvider,
                    OperationTimestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                };
                s_diagnosticListener.Write(RpcDiagnosticListenerNames.RpcFallbackBegin, eventData);
                return eventData.OperationTimestamp;
            }

            return null;
        }

        public void TracingFallbackAfter(long? tracingTimestamp, string messageId, string serviceEntryId,
            object result, IFallbackProvider fallbackProvider)
        {
            if (tracingTimestamp != null && s_diagnosticListener.IsEnabled(RpcDiagnosticListenerNames.RpcFallbackEnd))
            {
                var now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                var eventData = new FallbackResultEventData()
                {
                    ServiceEntryId = serviceEntryId,
                    Result = result,
                    MessageId = messageId,
                    FallbackProvider = fallbackProvider,
                    ElapsedTimeMs = now - DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                };
                s_diagnosticListener.Write(RpcDiagnosticListenerNames.RpcFallbackEnd, eventData);
            }
        }

        public void TracingFallbackError(long? tracingTimestamp, string messageId, string serviceEntryId,
            StatusCode statusCode,
            Exception ex, IFallbackProvider fallbackProvider)
        {
            if (tracingTimestamp != null && s_diagnosticListener.IsEnabled(RpcDiagnosticListenerNames.RpcFallbackError))
            {
                var now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                var eventData = new FallbackExceptionEventData()
                {
                    ServiceEntryId = serviceEntryId,
                    Exception = ex,
                    MessageId = messageId,
                    StatusCode = statusCode,
                    FallbackProvider = fallbackProvider,
                    ElapsedTimeMs = now - DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                };
                s_diagnosticListener.Write(RpcDiagnosticListenerNames.RpcFallbackError, eventData);
            }
        }
    }
}