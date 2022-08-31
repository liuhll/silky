using System.Threading.Tasks;
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
            await _channelContext.WriteAsync(message);
        }

        protected override async Task SendAndFlushAsync(TransportMessage message)
        {
            await _channelContext.WriteAndFlushAsync(message);
        }
    }
}