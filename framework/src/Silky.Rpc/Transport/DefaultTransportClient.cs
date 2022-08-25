using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Silky.Core;
using Silky.Core.Exceptions;
using Silky.Core.Extensions;
using Silky.Core.Runtime.Rpc;
using Silky.Rpc.Runtime;
using Silky.Rpc.Transport.Messages;

namespace Silky.Rpc.Transport
{
    public class DefaultTransportClient : ITransportClient
    {
        private static ConcurrentDictionary<string, TaskCompletionSource<TransportMessage>> m_resultDictionary = new();
        protected readonly IMessageSender _messageSender;
        protected readonly IMessageListener _messageListener;

        public ILogger<DefaultTransportClient> Logger { get; set; }

        public DefaultTransportClient(IMessageSender messageSender, IMessageListener messageListener)
        {
            _messageSender = messageSender;
            _messageListener = messageListener;
            _messageListener.Received += MessageListenerOnReceived;
            Logger = EngineContext.Current.Resolve<ILogger<DefaultTransportClient>>() ??
                     NullLogger<DefaultTransportClient>.Instance;
        }

        private async Task MessageListenerOnReceived(IMessageSender sender, TransportMessage message)
        {
            TaskCompletionSource<TransportMessage> task;
            if (!m_resultDictionary.TryGetValue(message.Id, out task))
                return;
            Debug.Assert(message.IsResultMessage(), "The message type received by the service consumer is incorrect");
            task.SetResult(message);
        }

        public virtual async Task<RemoteResultMessage> SendAsync(RemoteInvokeMessage message, string messageId,
            int timeout = Timeout.Infinite)
        {
            var transportMessage = new TransportMessage(message, messageId);
            transportMessage.SetRpcMessageId();
            message.Attachments = RpcContext.Context.GetInvokeAttachments();
            var callbackTask =
                RegisterResultCallbackAsync(transportMessage.Id, message.ServiceEntryId, timeout);
            Logger.LogDebug(
                "Preparing to send RPC message{0}" +
                "messageId:[{1}],serviceEntryId:[{2}]", Environment.NewLine, transportMessage.Id,
                message.ServiceEntryId);

            await _messageSender.SendMessageAsync(transportMessage);
            return await callbackTask;
        }

        private async Task<RemoteResultMessage> RegisterResultCallbackAsync(string id, string serviceEntryId,
            int timeout = Timeout.Infinite)
        {
            var tcs = new TaskCompletionSource<TransportMessage>();

            m_resultDictionary.TryAdd(id, tcs);
            RemoteResultMessage remoteResultMessage = null;
            try
            {
                var resultMessage = await tcs.WaitAsync(timeout);
                remoteResultMessage = resultMessage.GetContent<RemoteResultMessage>();
                Logger.LogDebug(
                    "Received the message returned from server{0}messageId:[{1}],serviceEntryId:[{2}]",
                    Environment.NewLine, id, serviceEntryId);
                CheckRemoteResultMessage(remoteResultMessage);
                return remoteResultMessage;
            }
            finally
            {
                RpcContext.Context.SetResultAttachments(remoteResultMessage?.Attachments);
                m_resultDictionary.TryRemove(id, out tcs);
            }
        }

        private void CheckRemoteResultMessage(RemoteResultMessage remoteResultMessage)
        {
            if (remoteResultMessage.StatusCode == StatusCode.Success)
            {
                return;
            }

            if (remoteResultMessage.StatusCode == StatusCode.ValidateError)
            {
                var validateException = new ValidationException(remoteResultMessage.ExceptionMessage);
                if (remoteResultMessage.ValidateErrors != null)
                {
                    foreach (var validateError in remoteResultMessage.ValidateErrors)
                    {
                        validateException.WithValidationError(validateError.ErrorMessage,
                            validateError.MemberNames);
                    }
                }

                throw validateException;
            }

            throw new SilkyException(remoteResultMessage.ExceptionMessage, remoteResultMessage.StatusCode,
                remoteResultMessage.Status);
        }
    }
}