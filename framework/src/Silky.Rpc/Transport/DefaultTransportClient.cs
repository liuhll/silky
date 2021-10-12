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
using Silky.Core.Logging;
using Silky.Core.Rpc;
using Silky.Rpc.Runtime;
using Silky.Rpc.Runtime.Client;
using Silky.Rpc.Transport.Messages;

namespace Silky.Rpc.Transport
{
    public class DefaultTransportClient : ITransportClient
    {
        private ConcurrentDictionary<string, TaskCompletionSource<TransportMessage>> m_resultDictionary = new();
        private readonly IMessageSender _messageSender;
        private readonly IMessageListener _messageListener;

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

        public async Task<RemoteResultMessage> SendAsync(RemoteInvokeMessage message, string messageId,
            int timeout = Timeout.Infinite)
        {
            var transportMessage = new TransportMessage(message, messageId);
            transportMessage.SetRpcMessageId();
            message.Attachments = RpcContext.Context.GetContextAttachments();
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
            try
            {
                var resultMessage = await tcs.WaitAsync(timeout);
                var remoteResultMessage = resultMessage.GetContent<RemoteResultMessage>();
                Logger.LogDebug(
                    "Received the message returned from server{0}messageId:[{1}],serviceEntryId:[{2}]",
                    Environment.NewLine, id, serviceEntryId);

                return remoteResultMessage;
            }
            finally
            {
                m_resultDictionary.TryRemove(id, out tcs);
            }
        }
    }
}