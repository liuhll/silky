using System.Net;
using DotNetty.Buffers;
using DotNetty.Handlers.Timeout;
using DotNetty.Transport.Channels;
using Silky.Core.Extensions;
using Silky.DotNetty.Abstraction;
using Silky.Rpc.Address.HealthCheck;

namespace Silky.DotNetty.Handlers
{
    public class ChannelInboundHandlerAdapter : ChannelHandlerAdapter
    {
        private readonly IHealthCheck _healthCheck;

        public ChannelInboundHandlerAdapter(IHealthCheck healthCheck)
        {
            _healthCheck = healthCheck;
        }

        public ChannelInboundHandlerAdapter()
        {
        }

        public override async void UserEventTriggered(IChannelHandlerContext context, object evt)
        {
            if (evt is IdleStateEvent { State: IdleState.ReaderIdle })
            {
                var remoteAddress = context.Channel.RemoteAddress as IPEndPoint;
                if (remoteAddress != null)
                {
                    _healthCheck?.ChangeHealthStatus(remoteAddress.Address.MapToIPv4(), remoteAddress.Port, false);
                }
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

        public override void ChannelRead(IChannelHandlerContext context, object message)
        {
            var remoteAddress = context.Channel.RemoteAddress as IPEndPoint;
            if (remoteAddress != null)
            {
                _healthCheck?.ChangeHealthStatus(remoteAddress.Address.MapToIPv4(), remoteAddress.Port, true);
            }

            base.ChannelRead(context, message);
        }
    }
}