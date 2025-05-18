using System;
using System.Net;
using System.Threading.Tasks;
using DotNetty.Buffers;
using DotNetty.Handlers.Timeout;
using DotNetty.Transport.Channels;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Silky.Core.Extensions;
using Silky.Core.Runtime.Rpc;
using Silky.DotNetty.Abstraction;
using Silky.Rpc.Endpoint.Monitor;

namespace Silky.DotNetty.Handlers
{
    public class ChannelInboundHandlerAdapter : ChannelHandlerAdapter
    {
        private readonly IRpcEndpointMonitor _rpcEndpointMonitor;

        public ILogger<ChannelInboundHandlerAdapter> Logger { get; set; }

        public ChannelInboundHandlerAdapter(IRpcEndpointMonitor rpcEndpointMonitor)
        {
            _rpcEndpointMonitor = rpcEndpointMonitor;
            Logger = NullLogger<ChannelInboundHandlerAdapter>.Instance;
        }

        public ChannelInboundHandlerAdapter()
        {
            Logger = NullLogger<ChannelInboundHandlerAdapter>.Instance;
        }
        
        public override void UserEventTriggered(IChannelHandlerContext context, object evt)
        {
            if (evt is IdleStateEvent idleEvent)
            {
                _ = HandleIdleEventAsync(context, idleEvent);
            }
            else
            {
                base.UserEventTriggered(context, evt);
            }
        }
        
        private async Task HandleIdleEventAsync(IChannelHandlerContext context, IdleStateEvent evt)
        {
            try
            {
                if (evt.State == IdleState.ReaderIdle)
                {
                    if (context.Channel.RemoteAddress is IPEndPoint remoteAddress)
                    {
                        _rpcEndpointMonitor?.ChangeStatus(remoteAddress.Address.MapToIPv4(),
                            remoteAddress.Port,
                            ServiceProtocol.Rpc,
                            false);
                    }
                }

                if (evt.State == IdleState.WriterIdle)
                {
                    var buffer = Unpooled.WrappedBuffer(HeartBeat.Semaphore.GetBytes());
                    await context.Channel.WriteAndFlushAsync(buffer);
                }
            }
            catch (Exception e)
            {
                Logger.LogError(e, "Error handling IdleStateEvent");
                try
                {
                    await context.CloseAsync();
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex, "Error closing context on failure");
                }
            }
        }
        
        public override void ChannelInactive(IChannelHandlerContext context)
        {
            if (context.Channel.RemoteAddress is IPEndPoint remoteAddress)
            {
                _rpcEndpointMonitor?.ChangeStatus(remoteAddress.Address.MapToIPv4(),
                    remoteAddress.Port,
                    ServiceProtocol.Rpc,
                    false);
            }

            base.ChannelInactive(context);
        }
        
    }
}