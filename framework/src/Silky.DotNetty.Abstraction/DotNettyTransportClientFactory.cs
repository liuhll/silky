using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Silky.Core;
using Silky.Core.Exceptions;
using Silky.Core.Logging;
using Silky.DotNetty.Abstraction;
using Silky.Rpc.Configuration;
using Silky.Rpc.Endpoint;
using Silky.Rpc.Endpoint.Monitor;
using Silky.Rpc.Runtime;
using Silky.Rpc.Runtime.Client;
using Silky.Rpc.Runtime.Server;
using Silky.Rpc.Transport;

namespace Silky.DotNetty
{
    public class DotNettyTransportClientFactory : ITransportClientFactory
    {
        public ILogger<DotNettyTransportClientFactory> Logger { get; set; }
        private readonly RpcOptions _rpcOptions;

        private readonly IRpcEndpointMonitor _rpcEndpointMonitor;

        private readonly IMessageListener _clientMessageListener;

        private readonly SilkyChannelPoolMap _silkyChannelPoolMap;
        private readonly IChannelProvider _channelProvider;
        private readonly IServerManager _serverManager;

        private ConcurrentDictionary<ISilkyEndpoint, ITransportClient> m_clients = new();

        public DotNettyTransportClientFactory(IRpcEndpointMonitor rpcEndpointMonitor,
            IOptions<RpcOptions> rpcOptions,
            SilkyChannelPoolMap silkyChannelPoolMap,
            IChannelProvider channelProvider,
            IServerManager serverManager)
        {
            _rpcEndpointMonitor = rpcEndpointMonitor;
            _silkyChannelPoolMap = silkyChannelPoolMap;
            _channelProvider = channelProvider;
            _serverManager = serverManager;
            _rpcEndpointMonitor.OnRemoveRpcEndpoint += endpoint =>
            {
                RemoveClient(endpoint);
                return Task.CompletedTask;
            };
            _serverManager.OnRemoveRpcEndpoint += (name, endpoint) =>
            {
                RemoveClient(endpoint);
                return Task.CompletedTask;
            };
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
                    var clientMessageSender =
                        await CreateClientMessageSender(silkyEndpoint, _rpcOptions.UseTransportClientPool);
                    client = new DefaultTransportClient(clientMessageSender, _clientMessageListener);
                    _ = m_clients.TryAdd(silkyEndpoint, client);
                    return client;
                }

                if (!client.MessageSender.Enabled)
                {
                    client.MessageSender.Dispose();

                    client.MessageSender =
                        await CreateClientMessageSender(silkyEndpoint, _rpcOptions.UseTransportClientPool);
                }

                return client;
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
                _rpcEndpointMonitor.RemoveRpcEndpoint(silkyEndpoint);
                throw new CommunicationException(ex.Message, ex);
            }
        }

        private async Task<IClientMessageSender> CreateClientMessageSender(ISilkyEndpoint silkyEndpoint,
            bool useTransportClientPool)
        {
            if (useTransportClientPool)
            {
                var pool = _silkyChannelPoolMap.Get(silkyEndpoint);
                var messageSender =
                    new ChannelPoolClientMessageSender(pool, _clientMessageListener, _rpcEndpointMonitor);
                return messageSender;
            }
            else
            {
                var channel =
                    await _channelProvider.Create(silkyEndpoint, _clientMessageListener, _rpcEndpointMonitor);
                var messageSender = new DotNettyClientMessageSender(channel);
                return messageSender;
            }
        }

        public void RemoveClient(ISilkyEndpoint silkyEndpoint)
        {
            if (m_clients.TryRemove(silkyEndpoint, out var client))
            {
                if (client is IDisposable disposable)
                {
                    disposable.Dispose();
                }
            }
            
        }


        public void Dispose()
        {
            // _silkyChannelPoolMap?.Dispose();
            m_clients.Clear();
            _silkyChannelPoolMap?.Dispose(); // 可选释放池
        }
    }
}