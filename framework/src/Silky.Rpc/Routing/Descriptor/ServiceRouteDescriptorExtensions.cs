using System.Linq;
using Silky.Rpc.Endpoint.Descriptor;

namespace Silky.Rpc.Routing.Descriptor
{
    public static class ServiceRouteDescriptorExtensions
    {
        public static ServiceRoute ConvertToServiceRoute(this ServiceRouteDescriptor serviceRouteDescriptor)
        {
            var serviceRoute = new ServiceRoute()
            {
                Service = serviceRouteDescriptor.Service,
                Endpoints = serviceRouteDescriptor.Addresses.Select(p => p.ConvertToAddressModel()).ToArray()
            };
            return serviceRoute;
        }
    }
}