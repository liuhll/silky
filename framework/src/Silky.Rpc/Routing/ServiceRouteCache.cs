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
using Silky.Rpc.Routing.Descriptor;
using Silky.Rpc.Runtime.Server;

namespace Silky.Rpc.Routing
{
    public class ServiceRouteCache : ISingletonDependency
    {
        private readonly ConcurrentDictionary<string, ServiceRoute> _serviceRouteCache = new();
        private readonly IHealthCheck _healthCheck;
        private readonly IServiceEntryManager _serviceEntryManager;

        public ILogger<ServiceRouteCache> Logger { get; set; }

        public event OnRemoveServiceRoutes OnRemoveServiceRoutes;

        public event OnRemoveServiceRoute OnRemoveServiceRoute;

        public ServiceRouteCache(IHealthCheck healthCheck,
            IServiceEntryManager serviceEntryManager)
        {
            _healthCheck = healthCheck;
            _serviceEntryManager = serviceEntryManager;
            _healthCheck.OnRemveAddress += OnRemoveAddressHandler;
            Logger = NullLogger<ServiceRouteCache>.Instance;
        }

        private async Task OnRemoveAddressHandler(IRpcAddress addressmodel)
        {
            addressmodel.InitFuseTimes();
            var removeAddressServiceRoutes =
                ServiceRoutes.Where(p => p.Addresses.Any(q => q.Descriptor == addressmodel.Descriptor));
            var updateRegisterServiceRouteDescriptors = new List<ServiceRouteDescriptor>();
            foreach (var removeAddressServiceRoute in removeAddressServiceRoutes)
            {
                removeAddressServiceRoute.Addresses =
                    removeAddressServiceRoute.Addresses.Where(p => p.Descriptor != addressmodel.Descriptor).ToArray();
                _serviceRouteCache.AddOrUpdate(removeAddressServiceRoute.Service.Id,
                    removeAddressServiceRoute, (id, _) => removeAddressServiceRoute);
                updateRegisterServiceRouteDescriptors.Add(removeAddressServiceRoute.ConvertToDescriptor());
            }

            OnRemoveServiceRoutes?.Invoke(updateRegisterServiceRouteDescriptors, addressmodel);
        }


        public void UpdateCache([NotNull] ServiceRouteDescriptor serviceRouteDescriptor)
        {
            Check.NotNull(serviceRouteDescriptor, nameof(serviceRouteDescriptor));
            var serviceRoute = serviceRouteDescriptor.ConvertToServiceRoute();
            Debug.Assert(serviceRoute != null, "serviceRoute != null");
            var cacheServiceRoute = _serviceRouteCache.GetValueOrDefault(serviceRouteDescriptor.Service.Id);
            if (serviceRoute == cacheServiceRoute)
            {
                Logger.LogDebug(
                    $"The cached routing data of [{serviceRoute.Service.Id}] is consistent with the routing data of the service registry, no need to update");
                return;
            }

            _serviceRouteCache[serviceRouteDescriptor.Service.Id] = serviceRoute;
            Logger.LogInformation(
                $"Update the service routing [{serviceRoute.Service.Id}] cache, the routing rpcAddress is:[{string.Join(',', serviceRoute.Addresses.Select(p => p.ToString()))}]");

            foreach (var address in serviceRoute.Addresses)
            {
                _healthCheck.Monitor(address);
            }

            var serviceEntries = _serviceEntryManager.GetServiceEntries(serviceRouteDescriptor.Service.Id);
            foreach (var serviceEntry in serviceEntries)
            {
                if (serviceEntry.FailoverCountIsDefaultValue)
                {
                    serviceEntry.GovernanceOptions.RetryTimes = serviceRouteDescriptor.Addresses.Count();
                    _serviceEntryManager.Update(serviceEntry);
                }
            }
        }

        public void RemoveCache(string serviceId)
        {
            _serviceRouteCache.TryRemove(serviceId, out ServiceRoute serviceRoute);
            if (serviceRoute != null)
            {
                foreach (var routeAddress in serviceRoute.Addresses)
                {
                    _healthCheck.RemoveAddress(routeAddress);
                }
            }
        }

        public IReadOnlyList<ServiceRouteDescriptor> ServiceRouteDescriptors =>
            _serviceRouteCache.Values.Select(p => p.ConvertToDescriptor()).ToImmutableArray();

        public IReadOnlyList<ServiceRoute> ServiceRoutes => _serviceRouteCache.Values.ToImmutableArray();

        public ServiceRoute GetServiceRoute(string serviceId)
        {
            if (_serviceRouteCache.TryGetValue(serviceId, out ServiceRoute serviceRoute))
            {
                return serviceRoute;
            }

            return null;
        }
    }
}