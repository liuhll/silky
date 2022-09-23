using System.Net;
using System.Threading;
using System.Threading.Tasks;
using DotNetty.Buffers;
using DotNetty.Transport.Channels;
using DotNetty.Transport.Channels.Pool;
using Silky.Core.Runtime.Rpc;
using Silky.Core.Threading;
using Silky.DotNetty.Handlers;
using Silky.Rpc.Endpoint.Monitor;
using Silky.Rpc.Runtime;
using Silky.Rpc.Transport.Messages;

namespace Silky.DotNetty.Abstraction;

public class ChannelPoolClientMessageSender : DotNettyMessageSenderBase
{
    private readonly IChannelPool _channelPool;
    private readonly IMessageListener _messageListener;
    private readonly IRpcEndpointMonitor _rpcEndpointMonitor;


    public ChannelPoolClientMessageSender(IChannelPool channelPool,
        IMessageListener messageListener,
        IRpcEndpointMonitor rpcEndpointMonitor)
    {
        _channelPool = channelPool;
        _messageListener = messageListener;
        _rpcEndpointMonitor = rpcEndpointMonitor;
    }

    protected override async Task SendAsync(TransportMessage message)
    {
        var channel = await _channelPool.AcquireAsync();
        try
        {
            SetChannelClientHandler(channel);
            SetClientPort(channel);
            var buffer = Unpooled.WrappedBuffer(message.Encode());
            await channel.WriteAsync(buffer);
        }
        finally
        {
            await _channelPool.ReleaseAsync(channel);
        }
    }


    protected override async Task SendAndFlushAsync(TransportMessage message)
    {
        var channel = await _channelPool.AcquireAsync();
        try
        {
            SetChannelClientHandler(channel);
            SetClientPort(channel);
            var buffer = Unpooled.WrappedBuffer(message.Encode());
            await channel.WriteAndFlushAsync(buffer);
        }
        finally
        {
            await _channelPool.ReleaseAsync(channel);
        }
    }

    private void SetChannelClientHandler(IChannel channel)
    {
        var pipeline = channel.Pipeline;
        if (pipeline.Get("ClientHandler") == null)
        {
            pipeline.AddLast("ClientHandler", new ClientHandler(_messageListener, _rpcEndpointMonitor));
        }
    }

    private void SetClientPort(IChannel channel)
    {
        if (channel.LocalAddress is IPEndPoint localAddress) 
        {
            RpcContext.Context.SetInvokeAttachment(AttachmentKeys.RpcRequestPort, localAddress.Port.ToString());
        }
    }
}