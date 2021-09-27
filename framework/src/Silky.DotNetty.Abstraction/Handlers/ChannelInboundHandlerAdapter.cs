using System.Net;
using DotNetty.Buffers;
using DotNetty.Handlers.Timeout;
using DotNetty.Transport.Channels;
using Silky.Core.Extensions;
using Silky.Core.Rpc;
using Silky.DotNetty.Abstraction;
using Silky.Rpc.Endpoint.Monitor;

namespace Silky.DotNetty.Handlers
{
    public class ChannelInboundHandlerAdapter : ChannelHandlerAdapter
    {
        private readonly IRpcEndpointMonitor _rpcEndpointMonitor;

        public ChannelInboundHandlerAdapter(IRpcEndpointMonitor rpcEndpointMonitor)
        {
            _rpcEndpointMonitor = rpcEndpointMonitor;
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
                    _rpcEndpointMonitor?.ChangeStatus(remoteAddress.Address.MapToIPv4(),
                        remoteAddress.Port,
                        ServiceProtocol.Tcp,
                        false);
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
                _rpcEndpointMonitor?.ChangeStatus(remoteAddress.Address.MapToIPv4(),
                    remoteAddress.Port,
                    ServiceProtocol.Tcp,
                    true);
            }

            base.ChannelRead(context, message);
        }
    }
}