using System.Diagnostics;
using Silky.Core.Exceptions;
using Silky.Rpc.Endpoint.Descriptor;
using Silky.Rpc.Routing.Descriptor;
using Silky.Rpc.Utils;

namespace Silky.Rpc.Runtime.Server
{
    public static class ServiceExtensions
    {
        public static ServiceRouteDescriptor CreateLocalRouteDescriptor(this Service service,
            RpcEndpointDescriptor rpcEndpointDescriptor)
        {
            if (!service.IsLocal)
            {
                throw new SilkyException("Only allow local service to generate routing descriptors");
            }

            return new ServiceRouteDescriptor()
            {
                Service = service.ServiceDescriptor,
                Addresses = new[]
                    { rpcEndpointDescriptor },
            };
        }
    }
}