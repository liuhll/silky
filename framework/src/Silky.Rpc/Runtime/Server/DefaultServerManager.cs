using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Silky.Core;
using Silky.Core.DependencyInjection;
using Silky.Core.Rpc;
using Silky.Rpc.Endpoint;
using Silky.Rpc.Endpoint.Monitor;

namespace Silky.Rpc.Runtime.Server
{
    public class DefaultServerManager : IServerManager, ISingletonDependency
    {
        private readonly ConcurrentDictionary<string, IServer> _serverCache = new();

        private readonly ConcurrentDictionary<string, ServiceEntryDescriptor> _serviceEntryDescriptors = new();

        private readonly ConcurrentDictionary<string, IRpcEndpoint[]> _rpcRpcEndpointCache = new();
        private readonly IRpcEndpointMonitor _rpcEndpointMonitor;

        public ILogger<DefaultServerManager> Logger { get; set; }

        public ServiceEntryDescriptor GetServiceEntryDescriptor(string serviceEntryId)
        {
            if (_serviceEntryDescriptors.TryGetValue(serviceEntryId, out var serviceEntryDescriptor))
            {
                return serviceEntryDescriptor;
            }

            serviceEntryDescriptor = _serverCache.Values
                .SelectMany(p => p.Services.SelectMany(p => p.ServiceEntries))
                .FirstOrDefault(p => p.Id == serviceEntryId);
            _serviceEntryDescriptors.TryAdd(serviceEntryId, serviceEntryDescriptor);
            return serviceEntryDescriptor;
        }

        public event OnRemoveRpcEndpoint OnRemoveRpcEndpoint;
        public event OnUpdateRpcEndpoint OnUpdateRpcEndpoint;

        public DefaultServerManager(IRpcEndpointMonitor rpcEndpointMonitor)
        {
            _rpcEndpointMonitor = rpcEndpointMonitor;
            _rpcEndpointMonitor.OnRemoveRpcEndpoint += RemoveRpcEndpointHandler;
            _rpcEndpointMonitor.OnStatusChange += HealthChangeHandler;
            Logger = NullLogger<DefaultServerManager>.Instance;
        }

        private async Task HealthChangeHandler(IRpcEndpoint rpcEndpoint, bool isHealth)
        {
            RemoveRpcEndpointCache(rpcEndpoint);
            if (isHealth)
            {
                rpcEndpoint.InitFuseTimes();
            }
        }

        private void RemoveRpcEndpointCache(IRpcEndpoint rpcEndpoint)
        {
            var needRemoveRpcEndpointKvs =
                _rpcRpcEndpointCache.Where(p => p.Value.Any(q => q.Host == rpcEndpoint.Host));
            foreach (var needRemoveRpcEndpointKv in needRemoveRpcEndpointKvs)
            {
                _rpcRpcEndpointCache.TryRemove(needRemoveRpcEndpointKv.Key, out _);
            }
        }

        private async Task RemoveRpcEndpointHandler(IRpcEndpoint rpcEndpoint)
        {
            var needRemoveEndpointServerRoutes =
                Servers.Where(p => p.Endpoints.Any(q => q.Host == rpcEndpoint.Host));

            foreach (var needRemoveEndpointServerRoute in needRemoveEndpointServerRoutes)
            {
                needRemoveEndpointServerRoute.Endpoints = needRemoveEndpointServerRoute.Endpoints
                    .Where(p => p.Descriptor != rpcEndpoint.Descriptor).ToArray();
                OnRemoveRpcEndpoint?.Invoke(needRemoveEndpointServerRoute.HostName, rpcEndpoint);
            }

            RemoveRpcEndpointCache(rpcEndpoint);
        }


        public void Update([NotNull] ServerDescriptor serverDescriptor)
        {
            Check.NotNull(serverDescriptor, nameof(serverDescriptor));
            var server = serverDescriptor.ConvertToServer();
            Debug.Assert(server != null, "serviceRoute != null");
            var cacheServer = _serverCache.GetValueOrDefault(serverDescriptor.HostName);
            if (server.Equals(cacheServer))
            {
                Logger.LogDebug(
                    "The cached server data of [{0}] is consistent with the routing data of the service registry, no need to update",
                    server.HostName);
                return;
            }

            _serverCache.AddOrUpdate(serverDescriptor.HostName, server, (k, v) => server);
            Logger.LogInformation(
                "Update the server [{0}] data cache," +
                "The instance endpoints of the server provider is: {1}[{2}]",
                server.HostName, Environment.NewLine, string.Join(',', server.Endpoints.Select(p => p.ToString())));

            foreach (var rpcEndpoint in server.Endpoints)
            {
                _rpcEndpointMonitor.Monitor(rpcEndpoint);
                RemoveRpcEndpointCache(rpcEndpoint);
            }

            OnUpdateRpcEndpoint?.Invoke(serverDescriptor.HostName, server.Endpoints.ToArray());
        }

        public void Remove(string hostName)
        {
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
            return _serverCache.GetValueOrDefault(hostName)?.ConvertToDescriptor();
        }

        public IServer GetServer(string hostName)
        {
            return _serverCache.GetValueOrDefault(hostName);
        }

        public IReadOnlyList<ServerDescriptor> ServerDescriptors =>
            _serverCache.Values.Select(p => p.ConvertToDescriptor()).ToArray();

        public IServer GetSelfServer()
        {
            if (_serverCache.TryGetValue(EngineContext.Current.HostName, out var server))
            {
                return server;
            }

            return server;
        }

        public IReadOnlyList<IServer> Servers => _serverCache.Values.ToArray();

        public IRpcEndpoint[] GetRpcEndpoints(string serviceId, ServiceProtocol serviceProtocol)
        {
            if (_rpcRpcEndpointCache.TryGetValue(serviceId, out IRpcEndpoint[] endpoints))
            {
                return endpoints;
            }

            endpoints = Servers.Where(p =>
                    p.Services.Any(q => q.Id == serviceId))
                .SelectMany(p => p.Endpoints.Where(e => e.ServiceProtocol == serviceProtocol))
                .Where(p => p.Enabled)
                .ToArray();
            if (endpoints.Any())
            {
                _rpcRpcEndpointCache.TryAdd(serviceId, endpoints);
            }

            return endpoints;
        }

        public ServiceDescriptor GetServiceDescriptor(string serviceId)
        {
            var serviceDescriptor = _serverCache.Values.SelectMany(p => p.Services)
                .FirstOrDefault(p => p.Id == serviceId);
            return serviceDescriptor;
        }
    }
}