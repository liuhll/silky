using DotNetty.Handlers.Timeout;
using DotNetty.Transport.Channels;

namespace Silky.DotNetty.Handlers
{
    public class ChannelInboundHandlerAdapter : ChannelHandlerAdapter
    {
        private int lossConnectCount = 0;

        public override async void UserEventTriggered(IChannelHandlerContext context, object evt)
        {
            if (evt is IdleStateEvent { State: IdleState.ReaderIdle })
            {
                lossConnectCount++;
                if (lossConnectCount <= 1) return;
                await context.Channel.CloseAsync();
            }
            else
            {
                base.UserEventTriggered(context, evt);
            }
        }

        public override void ChannelRead(IChannelHandlerContext context, object message)
        {
            lossConnectCount = 0;
            base.ChannelRead(context,message);
        }
        
    }
}