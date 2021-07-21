using System.Threading.Tasks;
using DotNetty.Buffers;
using DotNetty.Transport.Channels;
using Silky.Rpc.Messages;
using Silky.Rpc.Transport;

namespace Silky.DotNetty
{
    public class DotNettyTcpServerMessageSender : IMessageSender
    {
        private readonly IChannelHandlerContext _channelContext;
        public DotNettyTcpServerMessageSender(IChannelHandlerContext channelContext)
        {
            _channelContext = channelContext;
        }

        public async Task SendAsync(TransportMessage message)
        {
            var buffer = Unpooled.WrappedBuffer(message.Encode());
            await _channelContext.WriteAsync(buffer);
        }

        public async Task SendAndFlushAsync(TransportMessage message)
        {
            var buffer = Unpooled.WrappedBuffer(message.Encode());
            await _channelContext.WriteAndFlushAsync(buffer);
        }
    }
}