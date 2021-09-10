using System.Linq;
using Silky.Rpc.Routing.Descriptor;

namespace Silky.Rpc.Routing
{
    public static class ServiceRouteExtensions
    {
        public static RouteDescriptor ConvertToDescriptor(this ServiceRoute serviceRoute)
        {
            var descriptor = new RouteDescriptor()
            {
                Services = serviceRoute.Services,
                Addresses = serviceRoute.Addresses.Select(p => p.Descriptor).ToArray()
            };
            return descriptor;
        }
    }
}