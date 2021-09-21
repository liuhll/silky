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
using Silky.Rpc.Address;
using Silky.Rpc.Address.HealthCheck;
using Silky.Rpc.Endpoint;
using Silky.Rpc.Routing.Descriptor;
using Silky.Rpc.Runtime.Server;

namespace Silky.Rpc.Routing
{
    public class ServerRouteCache : ISingletonDependency
    {
        private readonly ConcurrentDictionary<string, ServerRoute> _serviceRouteCache = new();
        private readonly IHealthCheck _healthCheck;
        private readonly IServiceEntryManager _serviceEntryManager;

        public ILogger<ServerRouteCache> Logger { get; set; }

        public event OnRemoveServerRoutes OnRemoveServiceRoutes;

        public event OnRemoveServerRoute OnRemoveServiceRoute;

        public ServerRouteCache(IHealthCheck healthCheck,
            IServiceEntryManager serviceEntryManager)
        {
            _healthCheck = healthCheck;
            _serviceEntryManager = serviceEntryManager;
            _healthCheck.OnRemveAddress += OnRemoveAddressHandler;
            Logger = NullLogger<ServerRouteCache>.Instance;
        }

        private async Task OnRemoveAddressHandler(IRpcEndpoint addressmodel)
        {
            addressmodel.InitFuseTimes();
            var removeAddressServiceRoutes =
                ServiceRoutes.Where(p => p.Endpoints.Any(q => q.Descriptor == addressmodel.Descriptor));
            var updateRegisterServiceRouteDescriptors = new List<ServerRouteDescriptor>();
            foreach (var removeAddressServiceRoute in removeAddressServiceRoutes)
            {
                removeAddressServiceRoute.Endpoints =
                    removeAddressServiceRoute.Endpoints.Where(p => p.Descriptor != addressmodel.Descriptor).ToArray();
                _serviceRouteCache.AddOrUpdate(removeAddressServiceRoute.Service.Id,
                    removeAddressServiceRoute, (id, _) => removeAddressServiceRoute);
                updateRegisterServiceRouteDescriptors.Add(removeAddressServiceRoute.ConvertToDescriptor());
            }

            OnRemoveServiceRoutes?.Invoke(updateRegisterServiceRouteDescriptors, addressmodel);
        }


        public void UpdateCache([NotNull] ServerRouteDescriptor serverRouteDescriptor)
        {
            Check.NotNull(serverRouteDescriptor, nameof(serverRouteDescriptor));
            var serviceRoute = serverRouteDescriptor.ConvertToServiceRoute();
            Debug.Assert(serviceRoute != null, "serviceRoute != null");
            var cacheServiceRoute = _serviceRouteCache.GetValueOrDefault(serverRouteDescriptor.Service.Id);
            if (serviceRoute == cacheServiceRoute)
            {
                Logger.LogDebug(
                    $"The cached routing data of [{serviceRoute.Service.Id}] is consistent with the routing data of the service registry, no need to update");
                return;
            }

            _serviceRouteCache[serverRouteDescriptor.Service.Id] = serviceRoute;
            Logger.LogInformation(
                $"Update the service routing [{serviceRoute.Service.Id}] cache, the routing rpcEndpoint is:[{string.Join(',', serviceRoute.Endpoints.Select(p => p.ToString()))}]");

            foreach (var address in serviceRoute.Endpoints)
            {
                _healthCheck.Monitor(address);
            }

            var serviceEntries = _serviceEntryManager.GetServiceEntries(serverRouteDescriptor.Service.Id);
            foreach (var serviceEntry in serviceEntries)
            {
                if (serviceEntry.FailoverCountIsDefaultValue)
                {
                    serviceEntry.GovernanceOptions.RetryTimes = serverRouteDescriptor.Addresses.Count();
                    _serviceEntryManager.Update(serviceEntry);
                }
            }
        }

        public void RemoveCache(string serviceId)
        {
            _serviceRouteCache.TryRemove(serviceId, out ServerRoute serviceRoute);
            if (serviceRoute != null)
            {
                foreach (var routeAddress in serviceRoute.Endpoints)
                {
                    _healthCheck.RemoveRpcEndpoint(routeAddress);
                }
            }
        }

        public IReadOnlyList<ServerRouteDescriptor> ServiceRouteDescriptors =>
            _serviceRouteCache.Values.Select(p => p.ConvertToDescriptor()).ToImmutableArray();

        public IReadOnlyList<ServerRoute> ServiceRoutes => _serviceRouteCache.Values.ToImmutableArray();

        public ServerRoute GetServiceRoute(string serviceId)
        {
            if (_serviceRouteCache.TryGetValue(serviceId, out ServerRoute serviceRoute))
            {
                return serviceRoute;
            }

            return null;
        }
    }
}