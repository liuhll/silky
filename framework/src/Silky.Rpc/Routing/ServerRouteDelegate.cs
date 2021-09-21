using System.Collections.Generic;
using System.Threading.Tasks;
using Silky.Rpc.Address;
using Silky.Rpc.Endpoint;
using Silky.Rpc.Routing.Descriptor;

namespace Silky.Rpc.Routing
{
    public delegate Task OnRemoveServerRoutes(IEnumerable<ServerRouteDescriptor> serviceRouteDescriptors,
        IRpcEndpoint rpcEndpoint);

    public delegate Task OnRemoveServerRoute(string serviceId, IRpcEndpoint rpcEndpoint);
}