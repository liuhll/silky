using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Silky.Core;
using Silky.Core.Exceptions;
using Silky.Core.Rpc;
using Silky.Rpc.Diagnostics;
using Silky.Rpc.Messages;
using Silky.Rpc.Security;
using Silky.Rpc.Transport;

namespace Silky.Rpc.Runtime.Server
{
    public class DefaultServiceMessageReceivedHandler : IServiceMessageReceivedHandler
    {
        private readonly IServiceEntryLocator _serviceEntryLocator;

        protected static readonly DiagnosticListener s_diagnosticListener =
            new(RpcDiagnosticListenerNames.DiagnosticServerListenerName);

        public DefaultServiceMessageReceivedHandler(IServiceEntryLocator serviceEntryLocator)
        {
            _serviceEntryLocator = serviceEntryLocator;
        }

        public async Task Handle(string messageId, IMessageSender sender, RemoteInvokeMessage message)
        {
            RpcContext.Context
                .SetAttachments(message.Attachments);
            RpcContext.Context
                .SetAttachment(AttachmentKeys.IsGatewayHost, false);
            var tracingTimestamp = TracingBefore(message, messageId);
            RemoteResultMessage remoteResultMessage;
            var serviceEntry =
                _serviceEntryLocator.GetLocalServiceEntryById(message.ServiceId);
            try
            {
               
                if (serviceEntry == null)
                {
                    throw new NotFindLocalServiceEntryException($"Failed to get local service entry through service id {message.ServiceId}");
                }

                var tokenValidator = EngineContext.Current.Resolve<ITokenValidator>();
                if (!tokenValidator.Validate())
                {
                    throw new RpcAuthenticationException("rpc token is illegal");
                }

                var currentServiceKey = EngineContext.Current.Resolve<ICurrentServiceKey>();
                var result = await serviceEntry.Executor(currentServiceKey.ServiceKey,
                    message.Parameters);

                remoteResultMessage = new RemoteResultMessage()
                {
                    ServiceId = serviceEntry.Id,
                    Result = result,
                    StatusCode = StatusCode.Success
                };
                TracingAfter(tracingTimestamp, messageId, message.ServiceId, remoteResultMessage);
            }
            catch (Exception e)
            {
                remoteResultMessage = new RemoteResultMessage()
                {
                    ServiceId = serviceEntry?.Id,
                    ExceptionMessage = e.GetExceptionMessage(),
                    StatusCode = e.GetExceptionStatusCode(),
                    ValidateErrors = e.GetValidateErrors().ToArray()
                };
                TracingError(tracingTimestamp, messageId, message.ServiceId, e.GetExceptionStatusCode(), e);
            }

            var resultTransportMessage = new TransportMessage(remoteResultMessage, messageId);
            await sender.SendAndFlushAsync(resultTransportMessage);
        }

        private long? TracingBefore(RemoteInvokeMessage message, string messageId)
        {
            if (s_diagnosticListener.IsEnabled(RpcDiagnosticListenerNames.BeginRpcServerHandler))
            {
                var remoteAddress = RpcContext.Context.GetAttachment(AttachmentKeys.ServerAddress).ToString();
                var eventData = new RpcInvokeEventData()
                {
                    MessageId = messageId,
                    OperationTimestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                    ServiceId = message.ServiceId,
                    Message = message,
                    RemoteAddress = remoteAddress,
                    IsGateWay = RpcContext.Context.IsGateway(),
                    ServiceMethodName = RpcContext.Context.GetAttachment(AttachmentKeys.ServiceMethodName).ToString()
                };

                s_diagnosticListener.Write(RpcDiagnosticListenerNames.BeginRpcServerHandler, eventData);

                return eventData.OperationTimestamp;
            }

            return null;
        }

        private void TracingAfter(long? tracingTimestamp, string messageId, string serviceId, RemoteResultMessage remoteResultMessage)
        {
            if (tracingTimestamp != null &&
                s_diagnosticListener.IsEnabled(RpcDiagnosticListenerNames.EndRpcServerHandler))
            {
                var now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                var eventData = new RpcResultEventData()
                {
                    MessageId = messageId,
                    ServiceId = serviceId,
                    Result = remoteResultMessage.Result,
                    StatusCode = remoteResultMessage.StatusCode,
                    ElapsedTimeMs = now - tracingTimestamp.Value,
                    RemoteAddress = RpcContext.Context.GetAttachment(AttachmentKeys.ServerAddress).ToString()
                };

                s_diagnosticListener.Write(RpcDiagnosticListenerNames.EndRpcServerHandler, eventData);
            }
        }

        private void TracingError(long? tracingTimestamp, string messageId, string serviceId, StatusCode statusCode,
            Exception ex)
        {
            if (tracingTimestamp != null &&
                s_diagnosticListener.IsEnabled(RpcDiagnosticListenerNames.ErrorRpcServerHandler))
            {
                var now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                var eventData = new RpcExceptionEventData()
                {
                    MessageId = messageId,
                    ServiceId = serviceId,
                    StatusCode = statusCode,
                    ElapsedTimeMs = now - tracingTimestamp.Value,
                    RemoteAddress = RpcContext.Context.GetAttachment(AttachmentKeys.ServerAddress).ToString(),
                    Exception = ex
                };
                s_diagnosticListener.Write(RpcDiagnosticListenerNames.ErrorRpcServerHandler, eventData);
            }
        }
    }
}