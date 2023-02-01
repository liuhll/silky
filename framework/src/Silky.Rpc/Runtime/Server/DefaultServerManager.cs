using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Server.Kestrel.Core.Internal.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Primitives;
using Silky.Core;
using Silky.Core.DependencyInjection;
using Silky.Core.Extensions;
using Silky.Core.Runtime.Rpc;
using Silky.Rpc.Endpoint;
using Silky.Rpc.Endpoint.Monitor;

namespace Silky.Rpc.Runtime.Server
{
    public class DefaultServerManager : IServerManager, ISingletonDependency
    {
        private ConcurrentDictionary<string, IServer> _serverCache = null;

        private readonly ConcurrentDictionary<string, ServiceEntryDescriptor> _serviceEntryDescriptorCacheForId = new();

        private readonly ConcurrentDictionary<(string, HttpMethod), ServiceEntryDescriptor>
            _serviceEntryDescriptorCacheForApi = new();


        private readonly ConcurrentDictionary<string, ISilkyEndpoint[]> _rpcRpcEndpointCache = new();
        private readonly IRpcEndpointMonitor _rpcEndpointMonitor;

        private readonly object _lock;
        private IChangeToken _changeToken;
        private CancellationTokenSource _cancellationTokenSource;

        public ILogger<DefaultServerManager> Logger { get; set; }


        public DefaultServerManager(IRpcEndpointMonitor rpcEndpointMonitor)
        {
            _rpcEndpointMonitor = rpcEndpointMonitor;
            _rpcEndpointMonitor.OnRemoveRpcEndpoint += RemoveSilkyEndpointHandler;
            _rpcEndpointMonitor.OnStatusChange += HealthChangeHandler;
            Logger = NullLogger<DefaultServerManager>.Instance;

            _lock = new object();
        }

        public ServiceEntryDescriptor GetServiceEntryDescriptor(string serviceEntryId)
        {
            if (_serviceEntryDescriptorCacheForId.TryGetValue(serviceEntryId, out var serviceEntryDescriptor))
            {
                return serviceEntryDescriptor;
            }

            serviceEntryDescriptor = _serverCache?.Values
                .SelectMany(p => p.Services.SelectMany(p => p.ServiceEntries))
                .FirstOrDefault(p => p.Id == serviceEntryId);

            if (serviceEntryDescriptor != null)
            {
                _serviceEntryDescriptorCacheForId.TryAdd(serviceEntryId, serviceEntryDescriptor);
            }

            return serviceEntryDescriptor;
        }

        public ServiceEntryDescriptor GetServiceEntryDescriptor(string api, HttpMethod httpMethod)
        {
            if (_serviceEntryDescriptorCacheForApi.TryGetValue((api, httpMethod), out var serviceEntryDescriptor))
            {
                return serviceEntryDescriptor;
            }

            serviceEntryDescriptor = _serverCache?.Values
                .SelectMany(p => p.Services.SelectMany(p => p.ServiceEntries))
                .FirstOrDefault(p => p.WebApi == api && p.HttpMethod == httpMethod);
            if (serviceEntryDescriptor != null)
            {
                _serviceEntryDescriptorCacheForApi.TryAdd((api, httpMethod), serviceEntryDescriptor);
            }

            return serviceEntryDescriptor;
        }

        public event OnRemoveRpcEndpoint OnRemoveRpcEndpoint;
        public event OnUpdateRpcEndpoint OnUpdateRpcEndpoint;

        public IChangeToken GetChangeToken()
        {
            Initialize();
            Debug.Assert(_serverCache != null);
            Debug.Assert(_changeToken != null);
            return _changeToken;
        }

        private void Initialize()
        {
            if (_serverCache == null)
            {
                lock (_lock)
                {
                    if (_serverCache == null)
                    {
                        _serverCache = new ConcurrentDictionary<string, IServer>();
                        var oldCancellationTokenSource = _cancellationTokenSource;
                        _cancellationTokenSource = new CancellationTokenSource();
                        _changeToken = new CancellationChangeToken(_cancellationTokenSource.Token);
                        oldCancellationTokenSource?.Cancel();
                    }
                }
            }
        }


        private async Task HealthChangeHandler(ISilkyEndpoint silkyEndpoint, bool isHealth)
        {
            RemoveSilkyEndpointCache(silkyEndpoint);
            if (isHealth)
            {
                silkyEndpoint.InitFuseTimes();
            }
        }

        private void RemoveSilkyEndpointCache(ISilkyEndpoint silkyEndpoint)
        {
            var needRemoveRpcEndpointKvs =
                _rpcRpcEndpointCache.Where(p =>
                    p.Value.Any(q => q.GetAddress() == silkyEndpoint.GetAddress() || q.Host == silkyEndpoint.Host));
            foreach (var needRemoveRpcEndpointKv in needRemoveRpcEndpointKvs)
            {
                _rpcRpcEndpointCache.TryRemove(needRemoveRpcEndpointKv.Key, out _);
            }
        }

        private Task RemoveSilkyEndpointHandler(ISilkyEndpoint silkyEndpoint)
        {
            var needRemoveEndpointServerRoutes =
                Servers?.Where(p => p.Endpoints.Any(q => q.Host == silkyEndpoint.Host));

            if (needRemoveEndpointServerRoutes == null)
            {
                return Task.CompletedTask;
            }

            foreach (var needRemoveEndpointServerRoute in needRemoveEndpointServerRoutes)
            {
                needRemoveEndpointServerRoute.Endpoints = needRemoveEndpointServerRoute.Endpoints
                    .Where(p => p.Descriptor != silkyEndpoint.Descriptor).ToArray();
                OnRemoveRpcEndpoint?.Invoke(needRemoveEndpointServerRoute.HostName, silkyEndpoint);
            }

            RemoveSilkyEndpointCache(silkyEndpoint);
            return Task.CompletedTask;
        }


        public void Update([NotNull] ServerDescriptor serverDescriptor)
        {
            Check.NotNull(serverDescriptor, nameof(serverDescriptor));
            var server = serverDescriptor.ConvertToServer();
            Update(server);
        }

        public void UpdateAll([NotNull] ServerDescriptor[] serverDescriptors)
        {
            Check.NotNull(serverDescriptors, nameof(serverDescriptors));
            Initialize();

            foreach (var serverDescriptor in serverDescriptors)
            {
                var cacheServer = _serverCache.GetValueOrDefault(serverDescriptor.HostName);
                var server = serverDescriptor.ConvertToServer();
                if (server.Equals(cacheServer))
                {
                    Logger.LogDebug(
                        "The cached server data of [{0}] is consistent with the routing data of the service registry, no need to update",
                        server.HostName);
                    continue;
                }

                _serverCache.AddOrUpdate(server.HostName, server, (k, v) => server);
                Logger.LogInformation(
                    "Update the server [{0}] data cache," +
                    "The instance endpoints of the server provider is: {1}[{2}]",
                    server.HostName, Environment.NewLine, string.Join(';', server.Endpoints.Select(p => p.ToString())));

                foreach (var rpcEndpoint in server.Endpoints)
                {
                    _rpcEndpointMonitor.Monitor(rpcEndpoint);
                    RemoveSilkyEndpointCache(rpcEndpoint);
                }

                OnUpdateRpcEndpoint?.Invoke(server.HostName, server.Endpoints.ToArray());
            }

            var allCacheServerNames = _serverCache.Keys;
            var newServerNames = serverDescriptors.Select(p => p.HostName).Distinct();

            var needRemoveServerNames = allCacheServerNames.Except(newServerNames).ToArray();

            foreach (var needRemoveServerName in needRemoveServerNames)
            {
                _serverCache.TryRemove(needRemoveServerName, out _);
                Logger.LogInformation(
                    "Remove the server [{0}] route data cache.", needRemoveServerName);
            }

            var oldCancellationTokenSource = _cancellationTokenSource;
            _cancellationTokenSource = new CancellationTokenSource();
            _changeToken = new CancellationChangeToken(_cancellationTokenSource.Token);
            oldCancellationTokenSource?.Cancel();
        }

        public void Update([NotNull] IServer server)
        {
            Check.NotNull(server, nameof(server));
            Initialize();
            lock (_lock)
            {
                var cacheServer = _serverCache.GetValueOrDefault(server.HostName);
                if (server.Equals(cacheServer))
                {
                    Logger.LogDebug(
                        "The cached server data of [{0}] is consistent with the routing data of the service registry, no need to update",
                        server.HostName);
                    return;
                }

                _serverCache.AddOrUpdate(server.HostName, server, (k, v) => server);
                Logger.LogInformation(
                    "Update the server [{0}] data cache," +
                    "The instance endpoints of the server provider is: {1}[{2}]",
                    server.HostName, Environment.NewLine, string.Join(';', server.Endpoints.Select(p => p.ToString())));

                foreach (var rpcEndpoint in server.Endpoints)
                {
                    _rpcEndpointMonitor.Monitor(rpcEndpoint);
                    RemoveSilkyEndpointCache(rpcEndpoint);
                }

                OnUpdateRpcEndpoint?.Invoke(server.HostName, server.Endpoints.ToArray());

                var oldCancellationTokenSource = _cancellationTokenSource;
                _cancellationTokenSource = new CancellationTokenSource();
                _changeToken = new CancellationChangeToken(_cancellationTokenSource.Token);
                oldCancellationTokenSource?.Cancel();
            }
        }

        public void MakeFusing(ISilkyEndpoint silkyEndpoint, int breakerSeconds)
        {
            var servers = Servers?.Where(p => p.Endpoints.Any(e => e.Equals(silkyEndpoint)));
            if (servers?.Any() == true)
            {
                foreach (var server in servers)
                {
                    server.Endpoints.Single(e => e.Equals(silkyEndpoint)).MakeFusing(breakerSeconds);
                    Update(server);
                }
            }

            RemoveSilkyEndpointCache(silkyEndpoint);
        }

        public void Remove(string hostName)
        {
            if (_serverCache == null)
                return;
            _serverCache.TryRemove(hostName, out IServer server);
            if (server != null)
            {
                foreach (var routeAddress in server.Endpoints)
                {
                    _rpcEndpointMonitor.RemoveRpcEndpoint(routeAddress);
                }
            }
        }

        public ServerDescriptor GetServerDescriptor(string hostName)
        {
            return _serverCache?.GetValueOrDefault(hostName)?.ConvertToDescriptor();
        }

        public IServer GetServer(string hostName)
        {
            return _serverCache?.GetValueOrDefault(hostName);
        }

        public ServerDescriptor[]? ServerDescriptors
        {
            get
            {
                if (_serverCache == null)
                {
                    return Array.Empty<ServerDescriptor>();
                }

                return _serverCache.Values.Select(p => p.ConvertToDescriptor()).ToArray();
            }
        }

        public IServer GetSelfServer()
        {
            Debug.Assert(_serverCache != null, "_serverCache != null");
            if (_serverCache.TryGetValue(EngineContext.Current.HostName, out var server))
            {
                return server;
            }

            return server;
        }

        public IServer[]? Servers => _serverCache?.Values.ToArray();

        public ISilkyEndpoint[] GetRpcEndpoints(string serviceId, ServiceProtocol serviceProtocol)
        {
            ISilkyEndpoint[] CacheSilkyEndpoints()
            {
                var endpoints = Servers?.Where(p =>
                        p.Services.Any(q => q.Id == serviceId))
                    .SelectMany(p => p.Endpoints.Where(e => e.ServiceProtocol == serviceProtocol))
                    .Where(p => p.Enabled)
                    .ToArray();
                if (endpoints == null)
                {
                    return Array.Empty<ISilkyEndpoint>();
                }

                if (endpoints.Any())
                {
                    _rpcRpcEndpointCache.AddOrUpdate(serviceId, endpoints, (k, v) => v = endpoints);
                }

                return endpoints;
            }

            if (_rpcRpcEndpointCache.TryGetValue(serviceId, out var endpoints))
            {
                if (endpoints.Any(e => !e.Enabled))
                {
                    return CacheSilkyEndpoints();
                }

                var remoteAddress = RpcContext.Context.GetInvokeAttachment(AttachmentKeys.SelectedServerEndpoint);
                if (!remoteAddress.IsNullOrEmpty() &&
                    !endpoints.Any(e => e.GetAddress().Equals(remoteAddress) && e.Enabled))
                {
                    return CacheSilkyEndpoints();
                }

                return endpoints;
            }

            return CacheSilkyEndpoints();
        }


        public ServiceDescriptor GetServiceDescriptor(string serviceId)
        {
            if (_serverCache == null)
            {
                return null;
            }

            var serviceDescriptor = _serverCache.Values.SelectMany(p => p.Services)
                .FirstOrDefault(p => p.Id == serviceId);
            return serviceDescriptor;
        }
    }
}