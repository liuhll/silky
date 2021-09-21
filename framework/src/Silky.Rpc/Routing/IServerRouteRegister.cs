using System.Threading.Tasks;
using Silky.Core.Rpc;
using Silky.Rpc.Endpoint.Descriptor;

namespace Silky.Rpc.Routing
{
    public interface IServerRouteRegister
    {
        Task RegisterRpcRoutes(RpcEndpointDescriptor rpcEndpointDescriptor, ServiceProtocol serviceProtocol);
    }
}