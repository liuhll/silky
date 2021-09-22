using System.Linq;
using Silky.Rpc.Endpoint.Descriptor;

namespace Silky.Rpc.Routing.Descriptor
{
    public static class ServerRouteDescriptorExtensions
    {
        public static ServerRoute ConvertToServerRoute(this ServerRouteDescriptor serverRouteDescriptor)
        {
            var serviceRoute = new ServerRoute(serverRouteDescriptor.HostName)
            {
                Services = serverRouteDescriptor.Services.ToArray(),
                Endpoints = serverRouteDescriptor.Endpoints.Select(p => p.ConvertToRpcEndpoint()).ToArray()
            };
            return serviceRoute;
        }
    }
}