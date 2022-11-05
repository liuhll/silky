using System.Net;
using Silky.Core.DependencyInjection;
using Silky.Core.Runtime.Rpc;

namespace Silky.Rpc.Endpoint.Monitor
{
    public interface IRpcEndpointMonitor : ISingletonDependency
    {
        void Monitor(ISilkyEndpoint silkyEndpoint);

        bool IsEnable(ISilkyEndpoint silkyEndpoint);

        void ChangeStatus(ISilkyEndpoint silkyEndpoint, bool isEnable, int unHealthCeilingTimes = 0);

        void ChangeStatus(IPAddress mapToIPv4, int port, ServiceProtocol serviceProtocol, bool isEnable,
            int unHealthCeilingTimes = 0);

        void RemoveRpcEndpoint(ISilkyEndpoint silkyEndpoint);

        void RemoveRpcEndpoint(IPAddress ipAddress, int port);

        event StatusChangeEvent OnStatusChange;
        event RemoveSilkyEndpointEvent OnRemoveRpcEndpoint;
        event DisEnableEvent OnDisEnable;
        event AddMonitorEvent OnAddMonitor;
    }
}