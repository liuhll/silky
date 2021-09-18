using System.Net;
using System.Threading.Tasks;
using DotNetty.Buffers;
using DotNetty.Transport.Channels;
using Silky.Core.Rpc;
using Silky.DotNetty.Abstraction;
using Silky.Rpc.Transport.Messages;

namespace Silky.DotNetty
{
    public class DotNettyClientMessageSender : DotNettyMessageSenderBase
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
            var localAddress = _channel.LocalAddress as IPEndPoint;
            RpcContext.Context.SetAttachment(AttachmentKeys.RpcRequestPort, localAddress?.Port);
        }
    }
}