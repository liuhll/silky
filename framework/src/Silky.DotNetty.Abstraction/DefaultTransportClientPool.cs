using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DotNetty.Transport.Channels;
using DotNetty.Transport.Channels.Pool;
using Silky.Core.Threading;
using Silky.DotNetty.Handlers;
using Silky.Rpc.Endpoint.Monitor;
using Silky.Rpc.Runtime;
using Silky.Rpc.Transport;

namespace Silky.DotNetty.Abstraction;

public class DefaultTransportClientPool : IDisposable, ITransportClientPool
{
    private readonly IChannelPool _channelPool;
    private readonly IMessageListener _messageListener;
    private readonly IRpcEndpointMonitor _rpcEndpointMonitor;
    private readonly int _transportClientPoolNumber;
    private readonly Func<int, int, int> _generate;
    private readonly Random _random;
    private SemaphoreSlim _syncSemaphore { get; }

    private bool isFullPool = false;
    private IList<ITransportClient> m_clients = new List<ITransportClient>();
    private IList<IChannel> m_channels = new List<IChannel>();

    public DefaultTransportClientPool(IChannelPool channelPool,
        IMessageListener messageListener,
        IRpcEndpointMonitor rpcEndpointMonitor,
        int transportClientPoolNumber)
    {
        _channelPool = channelPool;
        _messageListener = messageListener;
        _rpcEndpointMonitor = rpcEndpointMonitor;
        _transportClientPoolNumber = transportClientPoolNumber;
        _random = new Random((int)DateTime.Now.Ticks);
        _generate = (min, max) => _random.Next(min, max);

        _syncSemaphore = new SemaphoreSlim(1, 1);
    }

    public async Task<ITransportClient> GetOrCreate()
    {
        if (isFullPool)
        {
            return m_clients[_generate(0, m_clients.Count)];
        }

        using (await _syncSemaphore.LockAsync())
        {
            var channel = await _channelPool.AcquireAsync();

            var pipeline = channel.Pipeline;
            pipeline.AddLast(new ClientHandler(_messageListener, _rpcEndpointMonitor));
            var messageSender = new DotNettyClientMessageSender(channel);
            var client = new DefaultTransportClient(messageSender, _messageListener);
            m_clients.Add(client);
            m_channels.Add(channel);
            isFullPool = m_clients.Count >= _transportClientPoolNumber;
            return client;
        }
    }

    public async void Dispose()
    {
        foreach (var channel in m_channels)
        {
            await _channelPool.ReleaseAsync(channel);
            await channel.CloseAsync();
        }
    }
}