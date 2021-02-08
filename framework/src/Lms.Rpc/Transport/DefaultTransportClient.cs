using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Lms.Core.Exceptions;
using Lms.Rpc.Messages;
using Microsoft.Extensions.Logging;

namespace Lms.Rpc.Transport
{
    public class DefaultTransportClient : ITransportClient
    {
        private ConcurrentDictionary<string, TaskCompletionSource<TransportMessage>> m_resultDictionary =
            new ConcurrentDictionary<string, TaskCompletionSource<TransportMessage>>();

        private readonly IMessageSender _messageSender;
        private readonly IMessageListener _messageListener;
        private readonly ILogger<DefaultTransportClient> _logger;

        public DefaultTransportClient(IMessageSender messageSender,
            IMessageListener messageListener,
            ILogger<DefaultTransportClient> logger)
        {
            _messageSender = messageSender;
            _messageListener = messageListener;
            _messageListener.Received += MessageListenerOnReceived;
            _logger = logger;
        }

        private async Task MessageListenerOnReceived(IMessageSender sender, TransportMessage message)
        {
            TaskCompletionSource<TransportMessage> task;
            if (!m_resultDictionary.TryGetValue(message.Id, out task))
                return;
            Debug.Assert(message.IsResultMessage(),"服务消费者接受到的消息类型不正确");
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

        public async Task<RemoteResultMessage> SendAsync(RemoteInvokeMessage message, int timeout)
        {
            var transportMessage = new TransportMessage(message);
            var callbackTask = RegisterResultCallbackAsync(transportMessage.Id);
            await _messageSender.SendAndFlushAsync(transportMessage);
            return await callbackTask;

        }

        private async Task<RemoteResultMessage> RegisterResultCallbackAsync(string id, int timeout = Timeout.Infinite)
        {
            var tcs = new TaskCompletionSource<TransportMessage>();
            m_resultDictionary.TryAdd(id, tcs);
            try
            {
                using (var clt = CancellationTokenSource.CreateLinkedTokenSource())
                {
                    if (timeout != Timeout.Infinite)
                    {
                        clt.CancelAfter(timeout);
                    }
                    clt.Token.Register(() => tcs.TrySetCanceled(), useSynchronizationContext: false);
                    var task = tcs.Task;
                    var resultMessage = await task;
                    return resultMessage.GetContent<RemoteResultMessage>();
                }
            }
            finally
            {
                m_resultDictionary.TryRemove(id, out tcs);
            }
        }
    }
}