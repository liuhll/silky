using System.Linq;
using Silky.Rpc.Address.Descriptor;

namespace Silky.Rpc.Routing.Descriptor
{
    public static class ServiceRouteDescriptorExtensions
    {
        public static ServiceRoute ConvertToServiceRoute(this ServiceRouteDescriptor serviceRouteDescriptor)
        {
            var serviceRoute = new ServiceRoute()
            {
                Service = serviceRouteDescriptor.Service,
                Addresses = serviceRouteDescriptor.Addresses.Select(p => p.ConvertToAddressModel()).ToArray()
            };
            return serviceRoute;
        }
    }
}