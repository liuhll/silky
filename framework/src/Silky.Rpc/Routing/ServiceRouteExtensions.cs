using System.Linq;
using Silky.Rpc.Routing.Descriptor;
using Silky.Rpc.Runtime.Server;
using Silky.Rpc.Utils;

namespace Silky.Rpc.Routing
{
    public static class ServiceRouteExtensions
    {
        public static ServiceRouteDescriptor ConvertToDescriptor(this ServiceRoute serviceRoute)
        {
            var descriptor = new ServiceRouteDescriptor()
            {
                Service = serviceRoute.Service,
                Addresses = serviceRoute.Endpoints.Select(p => p.Descriptor).ToArray()
            };
            return descriptor;
        }

        public static bool MultiServiceKeys(this ServiceRoute serviceRoute)
        {
            var serviceKeys = serviceRoute.Service.GetServiceKeys();
            return serviceKeys != null && serviceKeys.Any();
        }

        // public static int GetInstanceCount(this ServiceRoute serviceRoute)
        // {
        //     return serviceRoute.Endpoints.Count(p => p.ServiceProtocol == serviceRoute.Service.ServiceProtocol);
        // }
        //
        public static int GetInstanceCount(this ServiceRoute serviceRoute)
        {
            return serviceRoute.Endpoints.Where(p => SocketCheck.TestConnection(p.IPEndPoint))
                .Count(p => p.ServiceProtocol == serviceRoute.Service.ServiceProtocol);
        }
    }
}