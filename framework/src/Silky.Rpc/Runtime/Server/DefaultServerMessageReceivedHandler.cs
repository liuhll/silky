using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Silky.Core;
using Silky.Core.Exceptions;
using Silky.Core.Logging;
using Silky.Core.Rpc;
using Silky.Rpc.Diagnostics;
using Silky.Rpc.Security;
using Silky.Rpc.Transport.Messages;

namespace Silky.Rpc.Runtime.Server
{
    public class DefaultServerMessageReceivedHandler : IServerMessageReceivedHandler
    {
        private readonly IServiceEntryLocator _serviceEntryLocator;
        private readonly IServerHandleSupervisor _serverHandleSupervisor;
        public ILogger<DefaultServerMessageReceivedHandler> Logger { get; set; }

        private static readonly DiagnosticListener s_diagnosticListener =
            new(RpcDiagnosticListenerNames.DiagnosticServerListenerName);


        public DefaultServerMessageReceivedHandler(IServiceEntryLocator serviceEntryLocator,
            IServerHandleSupervisor serverHandleSupervisor)
        {
            _serviceEntryLocator = serviceEntryLocator;
            _serverHandleSupervisor = serverHandleSupervisor;
            Logger = NullLogger<DefaultServerMessageReceivedHandler>.Instance;
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

                _serverHandleSupervisor.Monitor((serviceEntry.Id, clientAddress));
                var currentServiceKey = EngineContext.Current.Resolve<ICurrentServiceKey>();
                var result = await serviceEntry.Executor(currentServiceKey.ServiceKey,
                    message.Parameters);

                remoteResultMessage.Result = result;
                remoteResultMessage.StatusCode = StatusCode.Success;
                TracingAfter(tracingTimestamp, messageId, message.ServiceEntryId, remoteResultMessage);
            }
            catch (Exception ex)
            {
                isHandleSuccess = false;
                remoteResultMessage.ExceptionMessage = ex.GetExceptionMessage();
                remoteResultMessage.StatusCode = ex.GetExceptionStatusCode();
                remoteResultMessage.ValidateErrors = ex.GetValidateErrors().ToArray();
                Logger.LogException(ex);
                TracingError(tracingTimestamp, messageId, message.ServiceEntryId, ex.GetExceptionStatusCode(), ex);
            }
            finally
            {
                sp.Stop();
                if (isHandleSuccess)
                {
                    _serverHandleSupervisor.ExecSuccess((serviceEntry?.Id, clientAddress), sp.ElapsedMilliseconds);
                }
                else
                {
                    _serverHandleSupervisor.ExecFail((serviceEntry?.Id, clientAddress),
                        !remoteResultMessage.StatusCode.IsFriendlyStatus(), sp.ElapsedMilliseconds);
                }
            }

            var resultTransportMessage = new TransportMessage(remoteResultMessage, messageId);
            await sender.SendMessageAsync(resultTransportMessage);
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

        private void TracingAfter(long? tracingTimestamp, string messageId, string serviceEntryId,
            RemoteResultMessage remoteResultMessage)
        {
            if (tracingTimestamp != null &&
                s_diagnosticListener.IsEnabled(RpcDiagnosticListenerNames.EndRpcServerHandler))
            {
                var now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                var eventData = new RpcInvokeResultEventData()
                {
                    MessageId = messageId,
                    ServiceEntryId = serviceEntryId,
                    Result = remoteResultMessage.Result,
                    StatusCode = remoteResultMessage.StatusCode,
                    ElapsedTimeMs = now - tracingTimestamp.Value
                };

                s_diagnosticListener.Write(RpcDiagnosticListenerNames.EndRpcServerHandler, eventData);
            }
        }

        private void TracingError(long? tracingTimestamp, string messageId, string serviceEntryId,
            StatusCode statusCode,
            Exception ex)
        {
            if (tracingTimestamp != null &&
                s_diagnosticListener.IsEnabled(RpcDiagnosticListenerNames.ErrorRpcServerHandler))
            {
                var now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                var eventData = new RpcInvokeExceptionEventData()
                {
                    MessageId = messageId,
                    ServiceEntryId = serviceEntryId,
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