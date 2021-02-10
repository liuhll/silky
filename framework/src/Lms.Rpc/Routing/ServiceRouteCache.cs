using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using Lms.Core;
using Lms.Core.DependencyInjection;
using Lms.Rpc.Address;
using Lms.Rpc.Address.HealthCheck;
using Lms.Rpc.Routing.Descriptor;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Lms.Rpc.Routing
{
    public class ServiceRouteCache : ISingletonDependency
    {
        private readonly ConcurrentDictionary<string, ServiceRoute> _serviceRouteCache = new();
        private readonly IHealthCheck _healthCheck;
        public ILogger<ServiceRouteCache> Logger { get; set; }

        public ServiceRouteCache(IHealthCheck healthCheck)
        {
            _healthCheck = healthCheck;
            _healthCheck.OnRemveAddress += HealthCheckOnOnRemveAddress;
            Logger = NullLogger<ServiceRouteCache>.Instance;
        }

        private async Task HealthCheckOnOnRemveAddress(IAddressModel addressmodel)
        {
            var remveAddressServiceRoutes =
                ServiceRoutes.Where(p => p.Addresses.Any(q => q.Descriptor == addressmodel.Descriptor));
            foreach (var remveAddressServiceRoute in remveAddressServiceRoutes)
            {
                remveAddressServiceRoute.Addresses =
                    remveAddressServiceRoute.Addresses.Where(p => p.Descriptor != addressmodel.Descriptor).ToArray();
                _serviceRouteCache.AddOrUpdate(remveAddressServiceRoute.ServiceDescriptor.Id,
                    remveAddressServiceRoute, (id, _) => remveAddressServiceRoute);
            }
        }


        public void UpdateCache([NotNull] ServiceRouteDescriptor serviceRouteDescriptor)
        {
            Check.NotNull(serviceRouteDescriptor, nameof(serviceRouteDescriptor));
            var serviceRoute = serviceRouteDescriptor.ConvertToServiceRoute();
            _serviceRouteCache.AddOrUpdate(serviceRouteDescriptor.ServiceDescriptor.Id,
                serviceRoute, (id, _) => serviceRoute);

            Logger.LogDebug(
                $"更新服务路由缓存,路由地址为:{string.Join(',', serviceRoute.Addresses.Select(p => p.ToString()))}");

            foreach (var address in serviceRoute.Addresses)
            {
                _healthCheck.Monitor(address);
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

        // public ServiceRoute this[string serviceId]
        // {
        //     get
        //     {
        //         if (_serviceRouteCache.TryGetValue(serviceId, out ServiceRoute serviceRoute))
        //         {
        //             return serviceRoute;
        //         }
        //
        //         return null;
        //     }
        // }
    }
}