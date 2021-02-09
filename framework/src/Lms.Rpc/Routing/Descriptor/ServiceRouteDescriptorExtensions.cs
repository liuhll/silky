using System.Collections.Generic;
using System.Linq;
using Lms.Rpc.Address;
using Lms.Rpc.Address.Descriptor;

namespace Lms.Rpc.Routing.Descriptor
{
    public static class ServiceRouteDescriptorExtensions
    {
        public static ServiceRoute ConvertToServiceRoute(this ServiceRouteDescriptor serviceRouteDescriptor)
        {
            var serviceRoute = new ServiceRoute()
            {
                ServiceDescriptor = serviceRouteDescriptor.ServiceDescriptor,
                Addresses = serviceRouteDescriptor.AddressDescriptors.Select(p => p.ConvertToAddressModel())
            };
            return serviceRoute;
        }
    }
}