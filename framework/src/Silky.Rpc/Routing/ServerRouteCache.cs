using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using Silky.Core;
using Silky.Core.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Silky.Rpc.Address.HealthCheck;
using Silky.Rpc.Endpoint;
using Silky.Rpc.Endpoint.Descriptor;
using Silky.Rpc.Routing.Descriptor;
using Silky.Rpc.Runtime.Server;

namespace Silky.Rpc.Routing
{
    public class ServerRouteCache : ISingletonDependency
    {
        private readonly ConcurrentDictionary<string, ServerRoute> _serverRouteCache = new();

        private readonly ConcurrentDictionary<string, IRpcEndpoint[]> _rpcRpcEndpointCache = new();
        private readonly IHealthCheck _healthCheck;
        private readonly IServiceEntryManager _serviceEntryManager;
        public ILogger<ServerRouteCache> Logger { get; set; }

        public event OnRemoveRpcEndpoint OnRemoveRpcEndpoint;

        public ServerRouteCache(IHealthCheck healthCheck,
            IServiceEntryManager serviceEntryManager)
        {
            _healthCheck = healthCheck;
            _serviceEntryManager = serviceEntryManager;
            _healthCheck.OnRemoveRpcEndpoint += RemoveRpcEndpointHandler;
            Logger = NullLogger<ServerRouteCache>.Instance;
        }

        private async Task RemoveRpcEndpointHandler(IRpcEndpoint rpcEndpoint)
        {
            rpcEndpoint.InitFuseTimes();
            var needRemoveEndpointServerRoutes =
                ServerRoutes.Where(p => p.Endpoints.Any(q => q.Descriptor == rpcEndpoint.Descriptor));

            foreach (var needRemoveEndpointServerRoute in needRemoveEndpointServerRoutes)
            {
                needRemoveEndpointServerRoute.Endpoints = needRemoveEndpointServerRoute.Endpoints
                    .Where(p => p.Descriptor != rpcEndpoint.Descriptor).ToArray();
                OnRemoveRpcEndpoint?.Invoke(needRemoveEndpointServerRoute.HostName, rpcEndpoint);
            }

            var needRemoveRpcEndpointKvs =
                _rpcRpcEndpointCache.Where(p => p.Value.Any(q => q.Descriptor == rpcEndpoint.Descriptor));
            foreach (var needRemoveRpcEndpointKv in needRemoveRpcEndpointKvs)
            {
                _rpcRpcEndpointCache.TryRemove(needRemoveRpcEndpointKv.Key, out _);
            }
        }


        public void UpdateCache([NotNull] ServerRouteDescriptor serverRouteDescriptor)
        {
            Check.NotNull(serverRouteDescriptor, nameof(serverRouteDescriptor));
            var serverRoute = serverRouteDescriptor.ConvertToServerRoute();
            Debug.Assert(serverRoute != null, "serviceRoute != null");
            var cacheServerRoute = _serverRouteCache.GetValueOrDefault(serverRouteDescriptor.HostName);
            if (serverRoute == cacheServerRoute)
            {
                Logger.LogDebug(
                    $"The cached routing data of [{serverRoute.HostName}] is consistent with the routing data of the service registry, no need to update");
                return;
            }

            _serverRouteCache[serverRouteDescriptor.HostName] = serverRoute;
            Logger.LogInformation(
                $"Update the service routing [{serverRoute.HostName}] cache," +
                $" the routing rpcEndpoint is:[{string.Join(',', serverRoute.Endpoints.Select(p => p.ToString()))}]");

            foreach (var rpcEndpoint in serverRoute.Endpoints)
            {
                _healthCheck.Monitor(rpcEndpoint);
            }

            var rpcEndPoints = serverRouteDescriptor.Endpoints.Select(p => p.ConvertToRpcEndpoint()).ToArray();
            foreach (var serviceDescriptor in serverRouteDescriptor.Services)
            {
                _rpcRpcEndpointCache.AddOrUpdate(serviceDescriptor.Id, rpcEndPoints, (k, v) => rpcEndPoints);
                var serviceEntries = _serviceEntryManager.GetServiceEntries(serviceDescriptor.Id);
                foreach (var serviceEntry in serviceEntries)
                {
                    if (serviceEntry.FailoverCountIsDefaultValue)
                    {
                        serviceEntry.GovernanceOptions.RetryTimes = serverRouteDescriptor.Endpoints.Count();
                        _serviceEntryManager.Update(serviceEntry);
                    }
                }
            }
        }

        public void RemoveCache(string hostName)
        {
            _serverRouteCache.TryRemove(hostName, out ServerRoute serviceRoute);
            if (serviceRoute != null)
            {
                foreach (var routeAddress in serviceRoute.Endpoints)
                {
                    _healthCheck.RemoveRpcEndpoint(routeAddress);
                }
            }
        }

        public IReadOnlyList<ServerRouteDescriptor> ServerRouteDescriptors =>
            _serverRouteCache.Values.Select(p => p.ConvertToDescriptor()).ToImmutableArray();

        public ServerRoute GetSelfServerRoute()
        {
            if (_serverRouteCache.TryGetValue(EngineContext.Current.HostName, out var serverRoute))
            {
                return serverRoute;
            }

            return serverRoute;
        }

        public IReadOnlyList<ServerRoute> ServerRoutes => _serverRouteCache.Values.ToImmutableArray();

        public IRpcEndpoint[] GetRpcEndpoints(string serviceId)
        {
            if (_rpcRpcEndpointCache.TryGetValue(serviceId, out IRpcEndpoint[] endpoints))
            {
                return endpoints;
            }

            endpoints = ServerRoutes.Where(p => p.Services.Any(p => p.Id == serviceId)).SelectMany(p => p.Endpoints)
                .ToArray();
            _rpcRpcEndpointCache.TryAdd(serviceId, endpoints);
            return endpoints;
        }

        public ServiceDescriptor GetServiceDescriptor(string serviceId)
        {
            var serviceDescriptor = _serverRouteCache.Values.SelectMany(p => p.Services)
                .FirstOrDefault(p => p.Id == serviceId);
            return serviceDescriptor;
        }
    }
}