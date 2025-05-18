using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Silky.Core;
using Silky.Core.Exceptions;
using Silky.Core.Extensions;
using Silky.Core.Runtime.Rpc;
using Silky.Rpc.Runtime;
using Silky.Rpc.Runtime.Client;
using Silky.Rpc.Transport.Messages;

namespace Silky.Rpc.Transport
{
    public class DefaultTransportClient : ITransportClient, IDisposable
    {
        private readonly ConcurrentDictionary<string, TaskCompletionSource<TransportMessage>> m_resultDictionary = new();
        private readonly ConcurrentDictionary<string, (TransportMessage Message, DateTime InsertedAt)> _earlyMessageBuffer = new();
        private readonly Timer _cleanupTimer;
        public ILogger<DefaultTransportClient> Logger { get; set; }

        public IClientMessageSender MessageSender { get; set; }
        private readonly IMessageListener _messageListener;

        public DefaultTransportClient(IClientMessageSender messageSender, IMessageListener messageListener)
        {
            MessageSender = messageSender;
            _messageListener = messageListener;
            _messageListener.Received += MessageListenerOnReceived;
            Logger = EngineContext.Current.Resolve<ILogger<DefaultTransportClient>>() ??
                     NullLogger<DefaultTransportClient>.Instance;
            _cleanupTimer = new Timer(CleanupExpiredMessages, null, TimeSpan.FromMinutes(1), TimeSpan.FromMinutes(1));
        }
        
        private void CleanupExpiredMessages(object? state)
        {
            var expiration = DateTime.UtcNow.AddSeconds(-30);
            foreach (var item in _earlyMessageBuffer)
            {
                if (item.Value.InsertedAt < expiration)
                {
                    _earlyMessageBuffer.TryRemove(item.Key, out _);
                }
            }
        }

        private async Task MessageListenerOnReceived(IMessageSender sender, TransportMessage message)
        {
            if (m_resultDictionary.TryGetValue(message.Id, out var tcs))
            {
                tcs.TrySetResult(message);
            }
            else
            {
                _earlyMessageBuffer[message.Id] = (message, DateTime.UtcNow);
            }
        }
        
        public virtual async Task<RemoteResultMessage> SendAsync(RemoteInvokeMessage message, string messageId,
            int timeout = Timeout.Infinite)
        {
            var transportMessage = new TransportMessage(message, messageId);
            transportMessage.SetRpcMessageId();
            message.Attachments = RpcContext.Context.GetInvokeAttachments();
            message.TransAttachments = RpcContext.Context.GetTransAttachments();
            var callbackTask =
                RegisterResultCallbackAsync(transportMessage.Id, message.ServiceEntryId, timeout);
            Logger.LogDebug(
                "Preparing to send RPC message{0}" +
                "messageId:[{1}],serviceEntryId:[{2}]", Environment.NewLine, transportMessage.Id,
                message.ServiceEntryId);
            try
            {
                await MessageSender.SendMessageAsync(transportMessage);
                return await callbackTask;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "RPC Send Message Failure，messageId:{MessageId}, serviceEntryId:{ServiceEntryId}",
                    transportMessage.Id, message.ServiceEntryId);
                throw;
            }
           
        }

        private async Task<RemoteResultMessage> RegisterResultCallbackAsync(string id, string serviceEntryId,
            int timeout = Timeout.Infinite)
        {
            var tcs = new TaskCompletionSource<TransportMessage>(TaskCreationOptions.RunContinuationsAsynchronously);
           
            RemoteResultMessage remoteResultMessage = null;
            try
            {
                m_resultDictionary.TryAdd(id, tcs);
                if (_earlyMessageBuffer.TryRemove(id, out var tuple))
                {
                    tcs.TrySetResult(tuple.Message);
                }
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
                if (remoteResultMessage != null)
                {
                    RpcContext.Context.SetResultAttachments(remoteResultMessage.Attachments);
                    RpcContext.Context.SetTransAttachments(remoteResultMessage.TransAttachments);
                }
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
                var validateException = new ValidationException(remoteResultMessage.ExceptionMessage,
                    remoteResultMessage.ValidateErrors);


                throw validateException;
            }

            throw new SilkyException(remoteResultMessage.ExceptionMessage, remoteResultMessage.StatusCode,
                remoteResultMessage.Status);
        }
        
        private bool _disposed;
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed) return;
            _disposed = true;
            if (disposing)
            {
                if (MessageSender is IDisposable disposable)
                {
                    disposable.Dispose();
                }
                _messageListener.Received -= MessageListenerOnReceived;
                _cleanupTimer?.Dispose();
            }
        }
    }
}