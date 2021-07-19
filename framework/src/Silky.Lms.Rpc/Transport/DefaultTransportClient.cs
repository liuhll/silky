using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Silky.Lms.Core.Exceptions;
using Silky.Lms.Core.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Silky.Lms.Rpc.Diagnostics;
using Silky.Lms.Rpc.Messages;

namespace Silky.Lms.Rpc.Transport
{
    public class DefaultTransportClient : ITransportClient
    {
        private ConcurrentDictionary<string, TaskCompletionSource<TransportMessage>> m_resultDictionary = new();

        private readonly IMessageSender _messageSender;
        private readonly IMessageListener _messageListener;

        protected static readonly DiagnosticListener s_diagnosticListener =
            new(RpcDiagnosticListenerNames.DiagnosticClientListenerName);


        public ILogger<DefaultTransportClient> Logger { get; set; }

        public DefaultTransportClient(IMessageSender messageSender,
            IMessageListener messageListener)
        {
            _messageSender = messageSender;
            _messageListener = messageListener;
            _messageListener.Received += MessageListenerOnReceived;
            Logger = NullLogger<DefaultTransportClient>.Instance;
        }

        private async Task MessageListenerOnReceived(IMessageSender sender, TransportMessage message)
        {
            TaskCompletionSource<TransportMessage> task;
            if (!m_resultDictionary.TryGetValue(message.Id, out task))
                return;
            Debug.Assert(message.IsResultMessage(), "服务消费者接受到的消息类型不正确");

            var content = message.GetContent<RemoteResultMessage>();
            if (content.StatusCode != StatusCode.Success)
            {
                if (content.StatusCode == StatusCode.ValidateError)
                {
                    var validateException = new ValidationException(content.ExceptionMessage);
                    if (content.ValidateErrors != null)
                    {
                        foreach (var validateError in content.ValidateErrors)
                        {
                            validateException.WithValidationError(validateError.ErrorMessage,
                                validateError.MemberNames);
                        }
                    }

                    task.TrySetException(validateException);
                }
                else
                {
                    var exception = new LmsException(content.ExceptionMessage, content.StatusCode);
                    task.TrySetException(exception);
                }
            }
            else
            {
                task.SetResult(message);
            }
        }

        public async Task<RemoteResultMessage> SendAsync(RemoteInvokeMessage message, int timeout = Timeout.Infinite)
        {
            message.Attachments = RpcContext.GetContext().GetContextAttachments();
            var transportMessage = new TransportMessage(message);
            var tracingTimestamp = TracingBefore(message, transportMessage.Id);
            var callbackTask =
                RegisterResultCallbackAsync(transportMessage.Id, message.ServiceId, tracingTimestamp, timeout);
            await _messageSender.SendAndFlushAsync(transportMessage);
            return await callbackTask;
        }

        private async Task<RemoteResultMessage> RegisterResultCallbackAsync(string id, string serviceId,
            long? tracingTimestamp, int timeout = Timeout.Infinite)
        {
            var tcs = new TaskCompletionSource<TransportMessage>();

            m_resultDictionary.TryAdd(id, tcs);
            try
            {
                var resultMessage = await tcs.WaitAsync(timeout);
                var remoteResultMessage = resultMessage.GetContent<RemoteResultMessage>();
                TracingAfter(tracingTimestamp, id, serviceId, remoteResultMessage.Result);
                return remoteResultMessage;
            }
            catch (Exception ex)
            {
                TracingError(tracingTimestamp, id, serviceId, ex.GetExceptionStatusCode(), ex);
                throw;
            }
            finally
            {
                m_resultDictionary.TryRemove(id, out tcs);
            }
        }

        private long? TracingBefore(RemoteInvokeMessage message, string messageId)
        {
            if (s_diagnosticListener.IsEnabled(RpcDiagnosticListenerNames.BeginRpcRequest))
            {
                var eventData = new RpcInvokeEventData()
                {
                    MessageId = messageId,
                    OperationTimestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                    ServiceId = message.ServiceId,
                    Message = message,
                    RemoteAddress = RpcContext.GetContext().GetAttachment(AttachmentKeys.RemoteAddress).ToString()
                };

                s_diagnosticListener.Write(RpcDiagnosticListenerNames.BeginRpcRequest, eventData);

                return eventData.OperationTimestamp;
            }

            return null;
        }

        private void TracingAfter(long? tracingTimestamp, string messageId, string serviceId, object result)
        {
            if (tracingTimestamp != null &&
                s_diagnosticListener.IsEnabled(RpcDiagnosticListenerNames.EndRpcRequest))
            {
                var now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                var eventData = new RpcResultEventData()
                {
                    MessageId = messageId,
                    Operation = serviceId,
                    Result = result,
                    ElapsedTimeMs = now - tracingTimestamp.Value,
                    RemoteAddress = RpcContext.GetContext().GetAttachment(AttachmentKeys.RemoteAddress).ToString()
                };

                s_diagnosticListener.Write(RpcDiagnosticListenerNames.EndRpcRequest, eventData);
            }
        }

        private void TracingError(long? tracingTimestamp, string messageId, string serviceId, StatusCode statusCode,
            Exception ex)
        {
            if (tracingTimestamp != null &&
                s_diagnosticListener.IsEnabled(RpcDiagnosticListenerNames.ErrorRpcRequest))
            {
                var now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                var eventData = new RpcExcetionEventData()
                {
                    MessageId = messageId,
                    Operation = serviceId,
                    StatusCode = statusCode,
                    ElapsedTimeMs = now - tracingTimestamp.Value,
                    RemoteAddress = RpcContext.GetContext().GetAttachment(AttachmentKeys.RemoteAddress).ToString(),
                    Exception = ex
                };
                s_diagnosticListener.Write(RpcDiagnosticListenerNames.ErrorRpcRequest, eventData);
            }
        }
    }
}