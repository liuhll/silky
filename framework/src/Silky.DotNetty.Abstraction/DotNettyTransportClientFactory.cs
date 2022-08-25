using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Silky.Core.Exceptions;
using Silky.Core.Logging;
using Silky.DotNetty.Abstraction;
using Silky.Rpc.Configuration;
using Silky.Rpc.Endpoint;
using Silky.Rpc.Endpoint.Monitor;
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


        private ConcurrentDictionary<IRpcEndpoint, DefaultTransportClient> m_clients = new();

        public DotNettyTransportClientFactory(IRpcEndpointMonitor rpcEndpointMonitor,
            SilkyChannelPoolMap silkyChannelPoolMap,
            IChannelProvider channelProvider,
            IOptions<RpcOptions> rpcOptions)
        {
            _rpcEndpointMonitor = rpcEndpointMonitor;
            _silkyChannelPoolMap = silkyChannelPoolMap;
            _channelProvider = channelProvider;
            _rpcOptions = rpcOptions.Value;
            Logger = NullLogger<DotNettyTransportClientFactory>.Instance;
        }


        public async Task<ITransportClient> GetClient(IRpcEndpoint rpcEndpoint)
        {
            try
            {
                Logger.LogDebug(
                    $"Ready to create a client for the server rpcEndpoint: {rpcEndpoint.IPEndPoint}.");

                if (!m_clients.TryGetValue(rpcEndpoint, out var client))
                {
                    var messageListener = new ClientMessageListener();
                    if (_rpcOptions.UseTransportClientPool)
                    {
                        var pool = _silkyChannelPoolMap.Get(rpcEndpoint);
                        var messageSender =
                            new ChannelPoolClientMessageSender(pool, messageListener, _rpcEndpointMonitor,
                                _rpcOptions.TransportClientPoolNumber);
                        client = new DefaultTransportClient(messageSender, messageListener);
                        _ = m_clients.GetOrAdd(rpcEndpoint, client);
                    }
                    else
                    {
                        var channel = await _channelProvider.Create(rpcEndpoint, messageListener, _rpcEndpointMonitor);
                        var messageSender = new DotNettyClientMessageSender(channel);
                        client = new DefaultTransportClient(messageSender, messageListener);
                    }
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