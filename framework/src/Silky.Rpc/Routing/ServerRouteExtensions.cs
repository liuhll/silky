using System.Linq;
using Silky.Rpc.Routing.Descriptor;

namespace Silky.Rpc.Routing
{
    public static class ServerRouteExtensions
    {
        public static ServerRouteDescriptor ConvertToDescriptor(this ServerRoute serverRoute)
        {
            var descriptor = new ServerRouteDescriptor()
            {
                HostName = serverRoute.HostName,
                Services = serverRoute.Services.ToArray(),
                Endpoints = serverRoute.Endpoints.Select(p => p.Descriptor).ToArray()
            };
            return descriptor;
        }
        
    }
}