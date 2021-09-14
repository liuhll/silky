using System.Threading.Tasks;
using DotNetty.Buffers;
using DotNetty.Transport.Channels;
using Silky.DotNetty.Abstraction;
using Silky.Rpc.Messages;

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
            var buffer = Unpooled.WrappedBuffer(message.Encode());
            await _channel.WriteAsync(buffer);
        }

        protected override async Task SendAndFlushAsync(TransportMessage message)
        {
            var buffer = Unpooled.WrappedBuffer(message.Encode());
            await _channel.WriteAndFlushAsync(buffer);
        }
    }
}