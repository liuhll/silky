using System.Threading.Tasks;
using DotNetty.Buffers;
using DotNetty.Transport.Channels;
using Silky.Lms.Rpc.Messages;
using Silky.Lms.Rpc.Transport;

namespace Silky.Lms.DotNetty
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