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
using Silky.Rpc.Address.HealthCheck;
using Silky.Rpc.Endpoint;

namespace Silky.Rpc.Runtime.Server
{
    public class DefaultServerManager : IServerManager, ISingletonDependency
    {
        private readonly ConcurrentDictionary<string, IServer> _serverCache = new();

        private readonly ConcurrentDictionary<string, IRpcEndpoint[]> _rpcRpcEndpointCache = new();
        private readonly IHealthCheck _healthCheck;
        private readonly IServiceEntryManager _serviceEntryManager;
        public ILogger<DefaultServerManager> Logger { get; set; }

        public event OnRemoveRpcEndpoint OnRemoveRpcEndpoint;

        public DefaultServerManager(IHealthCheck healthCheck,
            IServiceEntryManager serviceEntryManager)
        {
            _healthCheck = healthCheck;
            _serviceEntryManager = serviceEntryManager;
            _healthCheck.OnRemoveRpcEndpoint += RemoveRpcEndpointHandler;
            _healthCheck.OnHealthChange += HealthChangeHandler;
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
                _rpcRpcEndpointCache.Where(p => p.Value.Any(q => q.Descriptor == rpcEndpoint.Descriptor));
            foreach (var needRemoveRpcEndpointKv in needRemoveRpcEndpointKvs)
            {
                _rpcRpcEndpointCache.TryRemove(needRemoveRpcEndpointKv.Key, out _);
            }
        }

        private async Task RemoveRpcEndpointHandler(IRpcEndpoint rpcEndpoint)
        {
            var needRemoveEndpointServerRoutes =
                Servers.Where(p => p.Endpoints.Any(q => q.Descriptor == rpcEndpoint.Descriptor));

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
            var serverRoute = serverDescriptor.ConvertToServerRoute();
            Debug.Assert(serverRoute != null, "serviceRoute != null");
            var cacheServerRoute = _serverCache.GetValueOrDefault(serverDescriptor.HostName);
            if (serverRoute == cacheServerRoute)
            {
                Logger.LogDebug(
                    $"The cached routing data of [{serverRoute.HostName}] is consistent with the routing data of the service registry, no need to update");
                return;
            }

            _serverCache[serverDescriptor.HostName] = serverRoute;
            Logger.LogInformation(
                $"Update the server routing [{serverRoute.HostName}] cache," +
                $" the routing rpcEndpoint is:[{string.Join(',', serverRoute.Endpoints.Select(p => p.ToString()))}]");

            foreach (var rpcEndpoint in serverRoute.Endpoints)
            {
                _healthCheck.Monitor(rpcEndpoint);
            }

            foreach (var serviceDescriptor in serverDescriptor.Services)
            {
                var serviceEntries = _serviceEntryManager.GetServiceEntries(serviceDescriptor.Id);
                foreach (var serviceEntry in serviceEntries)
                {
                    if (serviceEntry.FailoverCountIsDefaultValue)
                    {
                        serviceEntry.GovernanceOptions.RetryTimes = serverDescriptor.Endpoints.Count();
                        _serviceEntryManager.Update(serviceEntry);
                    }
                }
            }
        }

        public void Remove(string hostName)
        {
            _serverCache.TryRemove(hostName, out IServer server);
            if (server != null)
            {
                foreach (var routeAddress in server.Endpoints)
                {
                    _healthCheck.RemoveRpcEndpoint(routeAddress);
                }
            }
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