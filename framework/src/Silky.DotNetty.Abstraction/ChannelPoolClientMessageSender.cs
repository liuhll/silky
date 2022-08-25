using System.Net;
using System.Threading;
using System.Threading.Tasks;
using DotNetty.Buffers;
using DotNetty.Transport.Channels;
using DotNetty.Transport.Channels.Pool;
using Silky.Core.Runtime.Rpc;
using Silky.DotNetty.Handlers;
using Silky.Rpc.Runtime;
using Silky.Rpc.Transport.Messages;

namespace Silky.DotNetty.Abstraction;

public class ChannelPoolClientMessageSender : DotNettyMessageSenderBase
{
    private readonly IChannelPool _channelPool;
    private readonly IMessageListener _messageListener;

    public ChannelPoolClientMessageSender(IChannelPool channelPool, IMessageListener messageListener)
    {
        _channelPool = channelPool;
        _messageListener = messageListener;
        new SemaphoreSlim(1, 1);
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
        if (pipeline.Get("clientHandler") == null)
        {
            pipeline.AddLast("clientHandler", new ClientHandler(_messageListener));
        }
    }

    private void SetClientPort(IChannel channel)
    {
        var localAddress = channel.LocalAddress as IPEndPoint;
        RpcContext.Context.SetInvokeAttachment(AttachmentKeys.RpcRequestPort, localAddress?.Port);
    }
}