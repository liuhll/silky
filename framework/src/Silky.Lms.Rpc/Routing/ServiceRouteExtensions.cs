using System.Linq;
using Silky.Lms.Rpc.Routing.Descriptor;

namespace Silky.Lms.Rpc.Routing
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