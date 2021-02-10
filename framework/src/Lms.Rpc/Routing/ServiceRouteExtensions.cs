using System.Linq;
using Lms.Rpc.Routing.Descriptor;

namespace Lms.Rpc.Routing
{
    public static class ServiceRouteExtensions
    {
        public static ServiceRouteDescriptor ConvertToDescriptor(this ServiceRoute serviceRoute)
        {
            var descriptor = new ServiceRouteDescriptor()
            {
                ServiceDescriptor = serviceRoute.ServiceDescriptor,
                AddressDescriptors = serviceRoute.Addresses.Select(p => p.Descriptor).ToArray()
            };
            return descriptor;
        }
    }
}