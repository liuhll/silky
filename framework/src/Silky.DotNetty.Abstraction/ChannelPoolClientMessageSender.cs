using System.Net;
using System.Threading.Tasks;
using DotNetty.Transport.Channels;
using DotNetty.Transport.Channels.Pool;
using Silky.Core.Exceptions;
using Silky.Core.Runtime.Rpc;
using Silky.DotNetty.Handlers;
using Silky.Rpc.Endpoint.Monitor;
using Silky.Rpc.Runtime;
using Silky.Rpc.Runtime.Client;
using Silky.Rpc.Transport.Messages;

namespace Silky.DotNetty.Abstraction;

public class ChannelPoolClientMessageSender : DotNettyMessageSenderBase, IClientMessageSender
{
    private readonly IChannelPool _channelPool;
    private readonly IMessageListener _messageListener;
    private readonly IRpcEndpointMonitor _rpcEndpointMonitor;
    private bool _enabled = true;
    private bool _disposed = false;


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
        IChannel? channel = null;
        try
        {
            channel = await _channelPool.AcquireAsync();
            if (channel == null)
            {
                throw new CommunicationException("No communication channel has been applied for");
            }

            _enabled = channel is { Active: true, IsWritable: true, Open: true, Registered: true };
            if (!_enabled)
            {
                throw new CommunicationException("The current channel cannot be used");
            }

            SetChannelClientHandler(channel);
            SetClientPort(channel);
            await channel.WriteAsync(message);
        }
        finally
        {
            if (channel != null)
            {
                await _channelPool.ReleaseAsync(channel);
            }
        }
    }


    protected override async Task SendAndFlushAsync(TransportMessage message)
    {
        IChannel? channel = null;

        try
        {
            channel = await _channelPool.AcquireAsync();
            if (channel == null)
            {
                throw new CommunicationException("No communication channel has been applied for");
            }

            _enabled = channel is { Active: true, IsWritable: true, Open: true, Registered: true };
            if (!_enabled)
            {
                throw new CommunicationException("The current channel cannot be used");
            }

            SetChannelClientHandler(channel);
            SetClientPort(channel);
            await channel.WriteAndFlushAsync(message);
        }
        finally
        {
            if (channel != null)
            {
                await _channelPool.ReleaseAsync(channel);
            }
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


    public bool Enabled => _enabled && !_disposed;

    public void Dispose()
    {
        _disposed = true;
    }
}