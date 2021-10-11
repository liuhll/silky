using System.Net;
using Silky.Core.DependencyInjection;
using Silky.Core.Rpc;

namespace Silky.Rpc.Endpoint.Monitor
{
    public interface IRpcEndpointMonitor : ISingletonDependency
    {
        void Monitor(IRpcEndpoint rpcEndpoint);

        bool IsEnable(IRpcEndpoint rpcEndpoint);

        void ChangeStatus(IRpcEndpoint rpcEndpoint, bool isEnable, int unHealthCeilingTimes = 0);

        void ChangeStatus(IPAddress mapToIPv4, int port, ServiceProtocol serviceProtocol, bool isEnable,
            int unHealthCeilingTimes = 0);

        void RemoveRpcEndpoint(IRpcEndpoint rpcEndpoint);

        void RemoveRpcEndpoint(IPAddress ipAddress, int port);

        event StatusChangeEvent OnStatusChange;
        event RemoveRpcEndpointEvent OnRemoveRpcEndpoint;
        event DisEnableEvent OnDisEnable;
        event AddMonitorEvent OnAddMonitor;
    }
}