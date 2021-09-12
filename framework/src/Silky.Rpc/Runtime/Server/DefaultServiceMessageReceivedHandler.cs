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

namespace Silky.Rpc.Runtime.Server
{
    public class DefaultServiceMessageReceivedHandler : IServiceMessageReceivedHandler
    {
        private readonly IServiceEntryLocator _serviceEntryLocator;
        private readonly IHandleSupervisor _handleSupervisor;

        private static readonly DiagnosticListener s_diagnosticListener =
            new(RpcDiagnosticListenerNames.DiagnosticServerListenerName);


        public DefaultServiceMessageReceivedHandler(IServiceEntryLocator serviceEntryLocator,
            IHandleSupervisor handleSupervisor)
        {
            _serviceEntryLocator = serviceEntryLocator;
            _handleSupervisor = handleSupervisor;
        }

        public async Task Handle(string messageId, IMessageSender sender, RemoteInvokeMessage message)
        {
            var sp = Stopwatch.StartNew();
            message.SetRpcAttachments();

            var clientAddress = RpcContext.Context.GetAttachment(AttachmentKeys.ClientAddress).ToString();
            var tracingTimestamp = TracingBefore(message, messageId);
            var serviceEntry =
                _serviceEntryLocator.GetLocalServiceEntryById(message.ServiceEntryId);
            var remoteResultMessage = new RemoteResultMessage()
            {
                ServiceEntryId = serviceEntry?.Id
            };
            var isHandleSuccess = true;
            try
            {
                if (serviceEntry == null)
                {
                    throw new NotFindLocalServiceEntryException(
                        $"Failed to get local service entry through serviceEntryId {message.ServiceEntryId}");
                }

                var tokenValidator = EngineContext.Current.Resolve<ITokenValidator>();
                if (!tokenValidator.Validate())
                {
                    throw new RpcAuthenticationException("rpc token is illegal");
                }

                _handleSupervisor.Monitor((serviceEntry.Id, clientAddress));
                var currentServiceKey = EngineContext.Current.Resolve<ICurrentServiceKey>();
                var result = await serviceEntry.Executor(currentServiceKey.ServiceKey,
                    message.Parameters);

                remoteResultMessage.Result = result;
                remoteResultMessage.StatusCode = StatusCode.Success;
                TracingAfter(tracingTimestamp, messageId, message.ServiceEntryId, remoteResultMessage);
            }
            catch (Exception e)
            {
                isHandleSuccess = false;
                remoteResultMessage.ExceptionMessage = e.GetExceptionMessage();
                remoteResultMessage.StatusCode = e.GetExceptionStatusCode();
                remoteResultMessage.ValidateErrors = e.GetValidateErrors().ToArray();
                TracingError(tracingTimestamp, messageId, message.ServiceEntryId, e.GetExceptionStatusCode(), e);
            }
            finally
            {
                sp.Stop();
                if (isHandleSuccess)
                {
                    _handleSupervisor.ExecSuccess((serviceEntry?.Id, clientAddress), sp.ElapsedMilliseconds);
                }
                else
                {
                    _handleSupervisor.ExecFail((serviceEntry?.Id, clientAddress),
                        !(remoteResultMessage.StatusCode.IsBusinessStatus() ||
                          remoteResultMessage.StatusCode.IsUnauthorized()), sp.ElapsedMilliseconds);
                }
            }

            var resultTransportMessage = new TransportMessage(remoteResultMessage, messageId);
            await sender.SendAndFlushAsync(resultTransportMessage);
        }

        private long? TracingBefore(RemoteInvokeMessage message, string messageId)
        {
            if (s_diagnosticListener.IsEnabled(RpcDiagnosticListenerNames.BeginRpcServerHandler))
            {
                var eventData = new RpcInvokeEventData()
                {
                    MessageId = messageId,
                    OperationTimestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                    ServiceEntryId = message.ServiceEntryId,
                    Message = message
                };

                s_diagnosticListener.Write(RpcDiagnosticListenerNames.BeginRpcServerHandler, eventData);

                return eventData.OperationTimestamp;
            }

            return null;
        }

        private void TracingAfter(long? tracingTimestamp, string messageId, string serviceId,
            RemoteResultMessage remoteResultMessage)
        {
            if (tracingTimestamp != null &&
                s_diagnosticListener.IsEnabled(RpcDiagnosticListenerNames.EndRpcServerHandler))
            {
                var now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                var eventData = new RpcInvokeResultEventData()
                {
                    MessageId = messageId,
                    ServiceEntryId = serviceId,
                    Result = remoteResultMessage.Result,
                    StatusCode = remoteResultMessage.StatusCode,
                    ElapsedTimeMs = now - tracingTimestamp.Value
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
                var eventData = new RpcInvokeExceptionEventData()
                {
                    MessageId = messageId,
                    ServiceEntryId = serviceId,
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