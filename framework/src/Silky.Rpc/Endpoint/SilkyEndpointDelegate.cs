using System.Threading.Tasks;

namespace Silky.Rpc.Endpoint
{
    public delegate Task StatusChangeEvent(ISilkyEndpoint silkyEndpoint, bool isEnable);

    public delegate Task DisEnableEvent(ISilkyEndpoint silkyEndpoint);

    public delegate Task RemoveSilkyEndpointEvent(ISilkyEndpoint silkyEndpoint);

    public delegate Task AddMonitorEvent(ISilkyEndpoint silkyEndpoint);
}