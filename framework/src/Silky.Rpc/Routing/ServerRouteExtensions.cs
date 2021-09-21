using System.Linq;
using Silky.Rpc.Routing.Descriptor;
using Silky.Rpc.Runtime.Server;
using Silky.Rpc.Utils;

namespace Silky.Rpc.Routing
{
    public static class ServerRouteExtensions
    {
        public static ServerRouteDescriptor ConvertToDescriptor(this ServerRoute serverRoute)
        {
            var descriptor = new ServerRouteDescriptor()
            {
                Service = serverRoute.Service,
                Addresses = serverRoute.Endpoints.Select(p => p.Descriptor).ToArray()
            };
            return descriptor;
        }

        public static bool MultiServiceKeys(this ServerRoute serverRoute)
        {
            var serviceKeys = serverRoute.Service.GetServiceKeys();
            return serviceKeys != null && serviceKeys.Any();
        }

        // public static int GetInstanceCount(this ServerRoute serverRoute)
        // {
        //     return serverRoute.Endpoints.Count(p => p.ServiceProtocol == serverRoute.Service.ServiceProtocol);
        // }
        //
        public static int GetInstanceCount(this ServerRoute serverRoute)
        {
            return serverRoute.Endpoints.Where(p => SocketCheck.TestConnection(p.IPEndPoint))
                .Count(p => p.ServiceProtocol == serverRoute.Service.ServiceProtocol);
        }
    }
}