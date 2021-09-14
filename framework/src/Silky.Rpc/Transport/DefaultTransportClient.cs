using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Silky.Core.Exceptions;
using Silky.Core.Extensions;
using Silky.Core.Rpc;
using Silky.Rpc.Diagnostics;
using Silky.Rpc.Messages;
using Silky.Rpc.Runtime;

namespace Silky.Rpc.Transport
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
                    var exception = new SilkyException(content.ExceptionMessage, content.StatusCode);
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
            message.Attachments = RpcContext.Context.GetContextAttachments();
            var transportMessage = new TransportMessage(message);
            var tracingTimestamp = TracingBefore(message, transportMessage.Id);
            var callbackTask =
                RegisterResultCallbackAsync(transportMessage.Id, message.ServiceEntryId, tracingTimestamp, timeout);
            await _messageSender.SendMessageAsync(transportMessage);
            return await callbackTask;
        }

        private async Task<RemoteResultMessage> RegisterResultCallbackAsync(string id, string serviceEntryId,
            long? tracingTimestamp, int timeout = Timeout.Infinite)
        {
            var tcs = new TaskCompletionSource<TransportMessage>();

            m_resultDictionary.TryAdd(id, tcs);
            try
            {
                var resultMessage = await tcs.WaitAsync(timeout);
                var remoteResultMessage = resultMessage.GetContent<RemoteResultMessage>();
                TracingAfter(tracingTimestamp, id, serviceEntryId, remoteResultMessage);
                return remoteResultMessage;
            }
            catch (Exception ex)
            {
                TracingError(tracingTimestamp, id, serviceEntryId, ex.GetExceptionStatusCode(), ex);
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
                    ServiceEntryId = message.ServiceEntryId,
                    Message = message
                };

                s_diagnosticListener.Write(RpcDiagnosticListenerNames.BeginRpcRequest, eventData);

                return eventData.OperationTimestamp;
            }

            return null;
        }

        private void TracingAfter(long? tracingTimestamp, string messageId, string serviceEntryId, RemoteResultMessage remoteResultMessage)
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
                    ElapsedTimeMs = now - tracingTimestamp.Value
                };

                s_diagnosticListener.Write(RpcDiagnosticListenerNames.EndRpcRequest, eventData);
            }
        }

        private void TracingError(long? tracingTimestamp, string messageId, string serviceEntryId, StatusCode statusCode,
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
                    RemoteAddress = RpcContext.Context.GetAttachment(AttachmentKeys.ServerAddress).ToString(),
                    Exception = ex
                };
                s_diagnosticListener.Write(RpcDiagnosticListenerNames.ErrorRpcRequest, eventData);
            }
        }
    }
}