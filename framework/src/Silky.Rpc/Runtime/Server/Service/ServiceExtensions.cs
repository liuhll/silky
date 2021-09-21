using Silky.Core.Exceptions;
using Silky.Rpc.Endpoint.Descriptor;
using Silky.Rpc.Routing.Descriptor;

namespace Silky.Rpc.Runtime.Server
{
    public static class ServiceExtensions
    {
        public static ServerRouteDescriptor CreateLocalRouteDescriptor(this Service service,
            RpcEndpointDescriptor rpcEndpointDescriptor)
        {
            if (!service.IsLocal)
            {
                throw new SilkyException("Only allow local service to generate routing descriptors");
            }

            return new ServerRouteDescriptor()
            {
                Service = service.ServiceDescriptor,
                Addresses = new[]
                    { rpcEndpointDescriptor },
            };
        }
    }
}