using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Lms.Core.DependencyInjection;
using Lms.Rpc.Routing.Descriptor;

namespace Lms.Rpc.Routing
{
    public class ServiceRouteCache : ISingletonDependency
    {
        private readonly ConcurrentDictionary<string, ServiceRoute> _serviceRouteCache =
            new ConcurrentDictionary<string, ServiceRoute>();

        public void UpdateCache(ServiceRouteDescriptor serviceRouteDescriptor)
        {
            _serviceRouteCache.AddOrUpdate(serviceRouteDescriptor.ServiceDescriptor.Id,
                serviceRouteDescriptor.ConvertToServiceRoute(),
                (id, _) => serviceRouteDescriptor.ConvertToServiceRoute());
        }

        public void RemoveCache(string serviceId)
        {
            _serviceRouteCache.TryRemove(serviceId, out ServiceRoute serviceRoute);
            if (serviceRoute != null)
            {
                foreach (var routeAddress in serviceRoute.Addresses)
                {
                    routeAddress.ChangeHealthState(false);
                }
            }
        }

        public IReadOnlyList<ServiceRouteDescriptor> ServiceRouteDescriptors =>
            _serviceRouteCache.Values.Select(p => p.ConvertToDescriptor()).ToImmutableArray();

        public IReadOnlyList<ServiceRoute> ServiceRoutes => _serviceRouteCache.Values.ToImmutableArray();

        public ServiceRoute this[string serviceId]
        {
            get
            {
                if (_serviceRouteCache.TryGetValue(serviceId, out ServiceRoute serviceRoute))
                {
                    return serviceRoute;
                }

                return null;
            }
        }
    }
}