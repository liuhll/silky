using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using Lms.Core.DependencyInjection;
using Lms.Rpc.Routing.Descriptor;

namespace Lms.Rpc.Routing
{
    public class ServiceRouteCache : ISingletonDependency
    {
        private readonly ConcurrentDictionary<string, ServiceRouteDescriptor> _serviceRouteDescriptorCache = new ConcurrentDictionary<string, ServiceRouteDescriptor>();
        private readonly ConcurrentDictionary<string, ServiceRoute> _serviceRouteCache = new ConcurrentDictionary<string, ServiceRoute>();
        
        public void AddDataSource(ServiceRouteDescriptor dataSource)
        {
            _serviceRouteDescriptorCache.GetOrAdd(dataSource.ServiceDescriptor.Id, dataSource);
            // _serviceRouteCache.GetOrAdd(dataSource.ServiceDescriptor.Id, dataSource);
        }

        public IReadOnlyList<ServiceRouteDescriptor> ServiceRouteDescriptors => _serviceRouteDescriptorCache.Values.ToImmutableArray();
    }
}