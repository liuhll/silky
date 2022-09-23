using System;
using System.Net;
using System.Threading.Tasks;
using DotNetty.Buffers;
using DotNetty.Transport.Channels;
using Silky.Core.Runtime.Rpc;
using Silky.DotNetty.Abstraction;
using Silky.Rpc.Transport.Messages;

namespace Silky.DotNetty
{
    public class DotNettyClientMessageSender : DotNettyMessageSenderBase, IDisposable
    {
        private readonly IChannel _channel;

        public DotNettyClientMessageSender(IChannel channel)
        {
            _channel = channel;
        }

        protected override async Task SendAsync(TransportMessage message)
        {
            SetClientPort();
            var buffer = Unpooled.WrappedBuffer(message.Encode());
            await _channel.WriteAsync(buffer);
        }

        protected override async Task SendAndFlushAsync(TransportMessage message)
        {
            SetClientPort();
            var buffer = Unpooled.WrappedBuffer(message.Encode());
            await _channel.WriteAndFlushAsync(buffer);
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
        }
    }
}