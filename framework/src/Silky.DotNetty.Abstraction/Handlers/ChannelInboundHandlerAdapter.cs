using DotNetty.Buffers;
using DotNetty.Handlers.Timeout;
using DotNetty.Transport.Channels;
using Silky.Core.Extensions;
using Silky.DotNetty.Abstraction;

namespace Silky.DotNetty.Handlers
{
    public class ChannelInboundHandlerAdapter : ChannelHandlerAdapter
    {
        public override async void UserEventTriggered(IChannelHandlerContext context, object evt)
        {
            if (evt is IdleStateEvent { State: IdleState.ReaderIdle })
            {
                await context.Channel.CloseAsync();
            }

            if (evt is IdleStateEvent { State: IdleState.WriterIdle })
            {
                var buffer = Unpooled.WrappedBuffer(HeartBeat.Semaphore.GetBytes());
                await context.Channel.WriteAndFlushAsync(buffer);
            }
            else
            {
                base.UserEventTriggered(context, evt);
            }
        }
        
    }
}