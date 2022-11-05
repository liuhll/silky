using System;
using System.Diagnostics;
using Silky.Core.Exceptions;
using Silky.Core.Runtime.Rpc;
using Silky.Rpc.Diagnostics;
using Silky.Rpc.Endpoint;
using Silky.Rpc.Endpoint.Selector;
using Silky.Rpc.Transport.Messages;

namespace Silky.Rpc.Runtime.Client
{
    public class DefaultClientDiagnosticListener : IClientInvokeDiagnosticListener
    {
        private static readonly DiagnosticListener s_diagnosticListener =
            new(RpcDiagnosticListenerNames.DiagnosticClientListenerName);

        public long? TracingBefore(RemoteInvokeMessage message, string messageId)
        {
            if (s_diagnosticListener.IsEnabled(RpcDiagnosticListenerNames.BeginRpcRequest))
            {
                var eventData = new RpcInvokeEventData()
                {
                    MessageId = messageId,
                    OperationTimestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                    ServiceEntryId = message.ServiceEntryId,
                    Message = message
                };

                s_diagnosticListener.Write(RpcDiagnosticListenerNames.BeginRpcRequest, eventData);

                return eventData.OperationTimestamp;
            }

            return null;
        }

        public void TracingAfter(long? tracingTimestamp, string messageId, string serviceEntryId,
            RemoteResultMessage remoteResultMessage)
        {
            if (tracingTimestamp != null &&
                s_diagnosticListener.IsEnabled(RpcDiagnosticListenerNames.EndRpcRequest))
            {
                var now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                var eventData = new RpcInvokeResultEventData()
                {
                    MessageId = messageId,
                    ServiceEntryId = serviceEntryId,
                    Result = remoteResultMessage.Result,
                    StatusCode = remoteResultMessage.StatusCode,
                    Status = remoteResultMessage.Status,
                    ElapsedTimeMs = now - tracingTimestamp.Value
                };

                s_diagnosticListener.Write(RpcDiagnosticListenerNames.EndRpcRequest, eventData);
            }
        }

        public void TracingError(long? tracingTimestamp, string messageId, string serviceEntryId,
            StatusCode statusCode,
            Exception ex)
        {
            if (tracingTimestamp != null &&
                s_diagnosticListener.IsEnabled(RpcDiagnosticListenerNames.ErrorRpcRequest))
            {
                var now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                var eventData = new RpcInvokeExceptionEventData()
                {
                    MessageId = messageId,
                    ServiceEntryId = serviceEntryId,
                    StatusCode = statusCode,
                    ElapsedTimeMs = now - tracingTimestamp.Value,
                    ClientAddress = RpcContext.Context.GetInvokeAttachment(AttachmentKeys.SelectedServerHost).ToString(),
                    Exception = ex
                };
                s_diagnosticListener.Write(RpcDiagnosticListenerNames.ErrorRpcRequest, eventData);
            }
        }

        public void TracingSelectInvokeAddress(long? tracingTimestamp, string serviceEntryId,
            ShuntStrategy shuntStrategy,
            ISilkyEndpoint[] rpcEndpoints,
            ISilkyEndpoint selectedSilkyEndpoint)
        {
            if (tracingTimestamp != null &&
                s_diagnosticListener.IsEnabled(RpcDiagnosticListenerNames.SelectInvokeAddress))
            {
                s_diagnosticListener.Write(RpcDiagnosticListenerNames.SelectInvokeAddress,
                    new SelectInvokeAddressEventData()
                    {
                        ServiceEntryId = serviceEntryId,
                        EnableRpcEndpoints = rpcEndpoints,
                        SelectedSilkyEndpoint = selectedSilkyEndpoint,
                        ShuntStrategy = shuntStrategy
                    });
            }
        }
    }
}