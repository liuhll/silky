using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using Lms.Core.DependencyInjection;
using Lms.Rpc.Routing.Descriptor;

namespace Lms.Rpc.Routing
{
    public class ServiceRouteCache : ISingletonDependency
    {
        private readonly ConcurrentDictionary<string, ServiceRouteDescriptor> _serviceRouteDescriptorCache =
            new ConcurrentDictionary<string, ServiceRouteDescriptor>();

        private readonly ConcurrentDictionary<string, ServiceRoute> _serviceRouteCache =
            new ConcurrentDictionary<string, ServiceRoute>();

        public void UpdateCache(ServiceRouteDescriptor serviceRouteDescriptor)
        {
            _serviceRouteDescriptorCache.AddOrUpdate(serviceRouteDescriptor.ServiceDescriptor.Id,
                serviceRouteDescriptor, 
                (id, _) => serviceRouteDescriptor);
            _serviceRouteCache.AddOrUpdate(serviceRouteDescriptor.ServiceDescriptor.Id, 
                serviceRouteDescriptor.ConvertToServiceRoute(),
                (id, _) => serviceRouteDescriptor.ConvertToServiceRoute());
        }

        public IReadOnlyList<ServiceRouteDescriptor> ServiceRouteDescriptors =>
            _serviceRouteDescriptorCache.Values.ToImmutableArray();

        public IReadOnlyList<ServiceRoute> ServiceRoutes => _serviceRouteCache.Values.ToImmutableArray();
    }
}