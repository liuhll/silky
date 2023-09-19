using System;
using System.Net;
using System.Threading.Tasks;
using DotNetty.Transport.Channels;
using Silky.Core.Runtime.Rpc;
using Silky.DotNetty.Abstraction;
using Silky.Rpc.Runtime.Client;
using Silky.Rpc.Transport.Messages;

namespace Silky.DotNetty
{
    public class DotNettyClientMessageSender : DotNettyMessageSenderBase, IClientMessageSender
    {
        private readonly IChannel _channel;
        private bool _disposed = false;

        public DotNettyClientMessageSender(IChannel channel)
        {
            _channel = channel;
        }

        protected override async Task SendAsync(TransportMessage message)
        {
            SetClientPort();
            await _channel.WriteAsync(message);
        }

        protected override async Task SendAndFlushAsync(TransportMessage message)
        {
            SetClientPort();
            await _channel.WriteAndFlushAsync(message);
        }

        private void SetClientPort()
        {
            if (_channel.LocalAddress is IPEndPoint localAddress)
            {
                RpcContext.Context.SetInvokeAttachment(AttachmentKeys.RpcRequestPort, localAddress.Port.ToString());
            }
        }

        public async void Dispose()
        {
            await _channel.CloseAsync();
            _disposed = true;
        }

        public bool Enabled => _channel.Active && !_disposed;
    }
}