using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using DotNetty.Transport.Channels;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Silky.Core.Exceptions;
using Silky.Core.Logging;
using Silky.DotNetty.Abstraction;
using Silky.Rpc.Configuration;
using Silky.Rpc.Endpoint;
using Silky.Rpc.Endpoint.Monitor;
using Silky.Rpc.Runtime;
using Silky.Rpc.Runtime.Client;
using Silky.Rpc.Transport;

namespace Silky.DotNetty
{
    public class DotNettyTransportClientFactory : IDisposable, ITransportClientFactory
    {
        public ILogger<DotNettyTransportClientFactory> Logger { get; set; }
        private readonly RpcOptions _rpcOptions;

        private readonly IRpcEndpointMonitor _rpcEndpointMonitor;
        private readonly SilkyChannelPoolMap _silkyChannelPoolMap;
        private readonly IChannelProvider _channelProvider;

        private readonly IMessageListener _clientMessageListener;

        private ConcurrentDictionary<ISilkyEndpoint, ITransportClient> m_clients = new();

        public DotNettyTransportClientFactory(IRpcEndpointMonitor rpcEndpointMonitor,
            SilkyChannelPoolMap silkyChannelPoolMap,
            IChannelProvider channelProvider,
            IOptions<RpcOptions> rpcOptions)
        {
            _rpcEndpointMonitor = rpcEndpointMonitor;
            _silkyChannelPoolMap = silkyChannelPoolMap;
            _channelProvider = channelProvider;
            _rpcOptions = rpcOptions.Value;
            _clientMessageListener = new ClientMessageListener();
            Logger = NullLogger<DotNettyTransportClientFactory>.Instance;
        }


        public async Task<ITransportClient> GetClient(ISilkyEndpoint silkyEndpoint)
        {
            try
            {
                Logger.LogDebug(
                    $"Ready to create a client for the server rpcEndpoint: {silkyEndpoint.IPEndPoint}.");

                if (!m_clients.TryGetValue(silkyEndpoint, out var client))
                {
                    if (_rpcOptions.UseTransportClientPool)
                    {
                        var pool = _silkyChannelPoolMap.Get(silkyEndpoint);
                        var messageSender =
                            new ChannelPoolClientMessageSender(pool, _clientMessageListener, _rpcEndpointMonitor);
                        client = new DefaultTransportClient(messageSender, _clientMessageListener);
                    }
                    else
                    {
                        var channel =
                            await _channelProvider.Create(silkyEndpoint, _clientMessageListener, _rpcEndpointMonitor);
                        var messageSender = new DotNettyClientMessageSender(channel);
                        client = new DefaultTransportClient(messageSender, _clientMessageListener);
                    }

                    _ = m_clients.TryAdd(silkyEndpoint, client);
                    return client;
                }

                if (_rpcOptions.UseTransportClientPool)
                {
                    var pool = _silkyChannelPoolMap.Get(silkyEndpoint);
                    var messageSender =
                        new ChannelPoolClientMessageSender(pool, _clientMessageListener, _rpcEndpointMonitor);
                    client.MessageSender = messageSender;
                }

                return client;
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
                throw new CommunicationException(ex.Message, ex);
            }
        }


        public void Dispose()
        {
            _silkyChannelPoolMap?.Dispose();
        }
    }
}