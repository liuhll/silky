using System.Threading.Tasks;
using Silky.Rpc.Endpoint.Descriptor;
using Silky.Rpc.Runtime.Server;

namespace Silky.Rpc.Routing
{
    public interface IServiceRouteRegister
    {
        Task RegisterRpcRoutes(RpcEndpointDescriptor rpcEndpointDescriptor, ServiceProtocol serviceProtocol);
    }
}