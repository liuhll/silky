using System.Threading.Tasks;

namespace Silky.Rpc.Endpoint
{
    public delegate Task StatusChangeEvent(IRpcEndpoint rpcEndpoint, bool isEnable);

    public delegate Task DisEnableEvent(IRpcEndpoint rpcEndpoint);

    public delegate Task RemoveRpcEndpointEvent(IRpcEndpoint rpcEndpoint);

    public delegate Task AddMonitorEvent(IRpcEndpoint rpcEndpoint);
}