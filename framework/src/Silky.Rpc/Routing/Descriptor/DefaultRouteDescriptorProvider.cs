using System.Linq;
using Silky.Core;
using Silky.Rpc.Address.Descriptor;
using Silky.Rpc.Runtime.Server;

namespace Silky.Rpc.Routing.Descriptor
{
    public class DefaultRouteDescriptorProvider : IRouteDescriptorProvider
    {
        private readonly IServiceManager _serviceManager;

        public DefaultRouteDescriptorProvider(IServiceManager serviceManager)
        {
            _serviceManager = serviceManager;
        }

        public RouteDescriptor Create(AddressDescriptor addressDescriptor, ServiceProtocol serviceProtocol)
        {
            var services = _serviceManager.GetLocalService(serviceProtocol);
            var routeDescriptor = new RouteDescriptor()
            {
                HostName = EngineContext.Current.HostName,
                Addresses = new[] { addressDescriptor },
                Services = services.Select(p => p.ServiceDescriptor)
            };
            return routeDescriptor;
        }
    }
}