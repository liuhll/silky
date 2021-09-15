using System.Linq;
using Silky.Rpc.Routing.Descriptor;

namespace Silky.Rpc.Routing
{
    public static class ServiceRouteExtensions
    {
        public static ServiceRouteDescriptor ConvertToDescriptor(this ServiceRoute serviceRoute)
        {
            var descriptor = new ServiceRouteDescriptor()
            {
                Service = serviceRoute.Service,
                Addresses = serviceRoute.Addresses.Select(p => p.Descriptor).ToArray()
            };
            return descriptor;
        }
    }
}