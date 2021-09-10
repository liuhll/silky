using System.Linq;
using Silky.Rpc.Address.Descriptor;

namespace Silky.Rpc.Routing.Descriptor
{
    public static class RouteDescriptorExtensions
    {
        public static ServiceRoute ConvertToServiceRoute(this RouteDescriptor routeDescriptor)
        {
            var serviceRoute = new ServiceRoute()
            {
                HostName = routeDescriptor.HostName,
                Services = routeDescriptor.Services.ToArray(),
                Addresses = routeDescriptor.Addresses.Select(p => p.ConvertToAddressModel()).ToArray()
            };
            return serviceRoute;
        }
    }
}