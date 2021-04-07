using System.Threading.Tasks;
using DotNetty.Buffers;
using DotNetty.Transport.Channels;
using Silky.Lms.Rpc.Messages;
using Silky.Lms.Rpc.Transport;

namespace Silky.Lms.DotNetty
{
    public class DotNettyClientMessageSender : IMessageSender
    {
        private readonly IChannel _channel;
        public DotNettyClientMessageSender(IChannel channel)
        {
            _channel = channel;
        }

        public async Task SendAsync(TransportMessage message)
        {
            var buffer = Unpooled.WrappedBuffer(message.Encode());
            await _channel.WriteAsync(buffer);
        }

        public async Task SendAndFlushAsync(TransportMessage message)
        {
            var buffer = Unpooled.WrappedBuffer(message.Encode());
            await _channel.WriteAndFlushAsync(buffer);
        }
    }
}