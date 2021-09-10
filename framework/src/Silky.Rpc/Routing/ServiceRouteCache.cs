using System.Collections.Concurrent;
using System.Collections.Generic;
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
        private readonly ConcurrentDictionary<string, IAddressModel[]> _serviceAddressCache = new();

        private readonly IHealthCheck _healthCheck;
        private readonly IServiceEntryLocator _serviceEntryLocator;
        private readonly IServiceEntryManager _serviceEntryManager;

        public ILogger<ServiceRouteCache> Logger { get; set; }

        public event OnRemoveServiceRoutes OnRemoveServiceRoutes;

        public event OnRemoveServiceRoute OnRemoveServiceRoute;

        public ServiceRouteCache(IHealthCheck healthCheck,
            IServiceEntryLocator serviceEntryLocator,
            IServiceEntryManager serviceEntryManager)
        {
            _healthCheck = healthCheck;
            _serviceEntryLocator = serviceEntryLocator;
            _serviceEntryManager = serviceEntryManager;
            _healthCheck.OnRemveAddress += OnRemoveAddressHandler;
            Logger = NullLogger<ServiceRouteCache>.Instance;
        }

        private async Task OnRemoveAddressHandler(IAddressModel addressmodel)
        {
            addressmodel.InitFuseTimes();
            var removeAddressServiceRoutes =
                ServiceRoutes.Where(p => p.Addresses.Any(q => q.Descriptor == addressmodel.Descriptor));
            var updateRegisterServiceRouteDescriptors = new List<RouteDescriptor>();
            foreach (var removeAddressServiceRoute in removeAddressServiceRoutes)
            {
                removeAddressServiceRoute.Addresses =
                    removeAddressServiceRoute.Addresses.Where(p => p.Descriptor != addressmodel.Descriptor).ToArray();
                _serviceRouteCache.AddOrUpdate(removeAddressServiceRoute.HostName,
                    removeAddressServiceRoute, (id, _) => removeAddressServiceRoute);
                updateRegisterServiceRouteDescriptors.Add(removeAddressServiceRoute.ConvertToDescriptor());
            }

            OnRemoveServiceRoutes?.Invoke(updateRegisterServiceRouteDescriptors, addressmodel);
        }


        public void UpdateCache([NotNull] RouteDescriptor routeDescriptor)
        {
            Check.NotNull(routeDescriptor, nameof(routeDescriptor));


            var serviceRoute = routeDescriptor.ConvertToServiceRoute();
            _serviceRouteCache.AddOrUpdate(routeDescriptor.HostName,
                serviceRoute, (id, _) => serviceRoute);

            Logger.LogDebug(
                $"Update the service routing [{serviceRoute.HostName}] cache, the routing address is:[{string.Join(',', serviceRoute.Addresses.Select(p => p.ToString()))}]");

            foreach (var address in serviceRoute.Addresses)
            {
                _healthCheck.Monitor(address);
            }

            var serviceEntry = _serviceEntryLocator.GetServiceEntryById(routeDescriptor.HostName);
            if (serviceEntry != null)
            {
                if (serviceEntry.FailoverCountIsDefaultValue)
                {
                    serviceEntry.GovernanceOptions.FailoverCount = routeDescriptor.Addresses.Count();
                    _serviceEntryManager.Update(serviceEntry);
                }
            }
        }

        public void RemoveCache(string hostName)
        {
            _serviceRouteCache.TryRemove(hostName, out ServiceRoute serviceRoute);
            if (serviceRoute != null)
            {
                foreach (var routeAddress in serviceRoute.Addresses)
                {
                    _healthCheck.RemoveAddress(routeAddress);
                }
            }
        }

        public IReadOnlyList<RouteDescriptor> ServiceRouteDescriptors =>
            _serviceRouteCache.Values.Select(p => p.ConvertToDescriptor()).ToArray();

        public IReadOnlyList<ServiceRoute> ServiceRoutes => _serviceRouteCache.Values.ToArray();

        public ServiceRoute GetServiceRoute([NotNull] string hostName)
        {
            Check.NotNull(hostName, nameof(hostName));
            if (_serviceRouteCache.TryGetValue(hostName, out ServiceRoute serviceRoute))
            {
                return serviceRoute;
            }

            return null;
        }

        public IAddressModel[] GetServiceAddress([NotNull] string serviceId)
        {
            Check.NotNull(serviceId, nameof(serviceId));
            return _serviceAddressCache.GetOrAdd(serviceId,
                ServiceRoutes.Where(p => p.Services.Any(s => s.Id == serviceId)).SelectMany(sr => sr.Addresses)
                    .ToArray());
        }
    }
}