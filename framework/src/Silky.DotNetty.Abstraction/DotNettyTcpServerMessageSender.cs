using System.Threading.Tasks;
using DotNetty.Buffers;
using DotNetty.Transport.Channels;
using Silky.DotNetty.Abstraction;
using Silky.Rpc.Transport.Messages;

namespace Silky.DotNetty
{
    public class DotNettyTcpServerMessageSender : DotNettyMessageSenderBase
    {
        private readonly IChannelHandlerContext _channelContext;

        public DotNettyTcpServerMessageSender(IChannelHandlerContext channelContext)
        {
            _channelContext = channelContext;
        }

        protected override async Task SendAsync(TransportMessage message)
        {
            var buffer = Unpooled.WrappedBuffer(message.Encode());
            await _channelContext.WriteAsync(buffer);
        }

        protected override async Task SendAndFlushAsync(TransportMessage message)
        {
            var buffer = Unpooled.WrappedBuffer(message.Encode());
            await _channelContext.WriteAndFlushAsync(buffer);
        }
    }
}