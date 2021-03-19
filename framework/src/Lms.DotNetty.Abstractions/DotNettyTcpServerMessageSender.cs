using System.Threading.Tasks;
using DotNetty.Buffers;
using DotNetty.Transport.Channels;
using Lms.Rpc.Messages;
using Lms.Rpc.Transport;

namespace Lms.DotNetty
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