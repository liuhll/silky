using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using DotNetty.Transport.Channels;
using DotNetty.Transport.Channels.Pool;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Silky.Core.Exceptions;
using Silky.Core.Logging;
using Silky.Core.Threading;
using Silky.DotNetty.Abstraction;
using Silky.Rpc.Endpoint;
using Silky.Rpc.Endpoint.Monitor;
using Silky.Rpc.Runtime.Client;
using Silky.Rpc.Transport;

namespace Silky.DotNetty
{
    public class DotNettyTransportClientFactory : IDisposable, ITransportClientFactory
    {
        public ILogger<DotNettyTransportClientFactory> Logger { get; set; }
        // private RpcOptions _rpcOptions;

        private readonly IRpcEndpointMonitor _rpcEndpointMonitor;
        private readonly SilkyChannelPoolMap _silkyChannelPoolMap;
        

        private ConcurrentDictionary<FixedChannelPool, DefaultTransportClient> m_clients = new();

        public DotNettyTransportClientFactory(IRpcEndpointMonitor rpcEndpointMonitor,
            SilkyChannelPoolMap silkyChannelPoolMap)
        {
            _rpcEndpointMonitor = rpcEndpointMonitor;
            _silkyChannelPoolMap = silkyChannelPoolMap;
            Logger = NullLogger<DotNettyTransportClientFactory>.Instance;
        }


        public ITransportClient GetClient(IRpcEndpoint rpcEndpoint)
        {
            try
            {
                Logger.LogDebug(
                    $"Ready to create a client for the server rpcEndpoint: {rpcEndpoint.IPEndPoint}.");

                var pool = _silkyChannelPoolMap.Get(rpcEndpoint);


                if (!m_clients.TryGetValue(pool, out var client))
                {
                    var messageListener = new ClientMessageListener();
                    var messageSender = new ChannelPoolClientMessageSender(pool,messageListener);
                    client = new DefaultTransportClient(messageSender, messageListener);

                    _ = m_clients.GetOrAdd(pool, client);
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