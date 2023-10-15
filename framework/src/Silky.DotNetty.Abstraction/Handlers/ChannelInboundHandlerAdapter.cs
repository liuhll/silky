using System;
using System.Net;
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

        public override async void UserEventTriggered(IChannelHandlerContext context, object evt)
        {
            try
            {
                if (evt is IdleStateEvent { State: IdleState.ReaderIdle })
                {
                    if (context.Channel.RemoteAddress is IPEndPoint remoteAddress)
                    {
                        _rpcEndpointMonitor?.ChangeStatus(remoteAddress.Address.MapToIPv4(),
                            remoteAddress.Port,
                            ServiceProtocol.Rpc,
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
            catch (Exception e)
            {
                Logger.LogError(e.Message, e);
                try
                {
                    await context.CloseAsync();
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex.Message, ex);
                }
            }
        }

        public override void ChannelRead(IChannelHandlerContext context, object message)
        {
            if (context.Channel.RemoteAddress is IPEndPoint remoteAddress)
            {
                _rpcEndpointMonitor?.ChangeStatus(remoteAddress.Address.MapToIPv4(),
                    remoteAddress.Port,
                    ServiceProtocol.Rpc,
                    true);
            }

            base.ChannelRead(context, message);
        }
    }
}