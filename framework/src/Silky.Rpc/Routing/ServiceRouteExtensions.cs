using System.Linq;
using Silky.Rpc.Routing.Descriptor;
using Silky.Rpc.Runtime.Server;

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

        public static bool MultiServiceKeys(this ServiceRoute serviceRoute)
        {
            var serviceKeys = serviceRoute.Service.GetServiceKeys();
            return serviceKeys != null && serviceKeys.Any();
        }
    }
}