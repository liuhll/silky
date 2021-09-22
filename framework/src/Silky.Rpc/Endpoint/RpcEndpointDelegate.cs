using System.Threading.Tasks;

namespace Silky.Rpc.Endpoint
{
    public delegate Task HealthChangeEvent(IRpcEndpoint rpcEndpoint, bool isHealth);

    public delegate Task UnhealthEvent(IRpcEndpoint rpcEndpoint);

    public delegate Task RemoveRpcEndpointEvent(IRpcEndpoint rpcEndpoint);

    public delegate Task AddMonitorEvent(IRpcEndpoint rpcEndpoint);
    
}