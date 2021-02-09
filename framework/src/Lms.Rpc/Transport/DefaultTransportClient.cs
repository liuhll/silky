using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Lms.Core.Exceptions;
using Lms.Core.Extensions;
using Lms.Rpc.Messages;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Lms.Rpc.Transport
{
    public class DefaultTransportClient : ITransportClient
    {
        private ConcurrentDictionary<string, TaskCompletionSource<TransportMessage>> m_resultDictionary = new();
            

        private readonly IMessageSender _messageSender;
        private readonly IMessageListener _messageListener;
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
                task.TrySetException(new LmsException(content.ExceptionMessage, content.StatusCode));
            }
            else
            {
                task.SetResult(message);
            }
        }

        public async Task<RemoteResultMessage> SendAsync(RemoteInvokeMessage message, int timeout = Timeout.Infinite)
        {
            var transportMessage = new TransportMessage(message);
            var callbackTask = RegisterResultCallbackAsync(transportMessage.Id, timeout);
            await _messageSender.SendAndFlushAsync(transportMessage);
            return await callbackTask;
        }

        private async Task<RemoteResultMessage> RegisterResultCallbackAsync(string id, int timeout = Timeout.Infinite)

        {
            var tcs = new TaskCompletionSource<TransportMessage>();
            m_resultDictionary.TryAdd(id, tcs);
            try
            {
                var resultMessage = await tcs.WaitAsync(timeout);
                return resultMessage.GetContent<RemoteResultMessage>();
            }
            finally
            {
                m_resultDictionary.TryRemove(id, out tcs);
            }
        }
    }
}