using System.Linq;
using Silky.Rpc.Endpoint.Descriptor;

namespace Silky.Rpc.Routing.Descriptor
{
    public static class ServerRouteDescriptorExtensions
    {
        public static ServerRoute ConvertToServiceRoute(this ServerRouteDescriptor serverRouteDescriptor)
        {
            var serviceRoute = new ServerRoute()
            {
                Service = serverRouteDescriptor.Service,
                Endpoints = serverRouteDescriptor.Addresses.Select(p => p.ConvertToAddressModel()).ToArray()
            };
            return serviceRoute;
        }
    }
}