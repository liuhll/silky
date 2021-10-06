using System.Threading.Tasks;
using Silky.Rpc.Endpoint;
using Silky.Rpc.Endpoint.Descriptor;

namespace Silky.Rpc.HealthCheck
{
    public interface IServerHealthCheck
    {
        Task<bool> IsHealth(string address);
        
        Task<bool> IsHealth(IRpcEndpoint rpcEndpoint);
        
        Task<bool> IsHealth(RpcEndpointDescriptor rpcEndpointDescriptor);
    }
}