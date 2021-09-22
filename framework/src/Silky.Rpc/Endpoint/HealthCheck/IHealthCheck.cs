using System.Net;
using Silky.Core.DependencyInjection;
using Silky.Core.Rpc;
using Silky.Rpc.Endpoint;
using Silky.Rpc.Endpoint.Descriptor;
using Silky.Rpc.Routing;

namespace Silky.Rpc.Address.HealthCheck
{
    public interface IHealthCheck : ISingletonDependency
    {
        void Monitor(IRpcEndpoint rpcEndpoint);

        bool IsHealth(IPEndPoint ipEndPoint);

        bool IsHealth(RpcEndpointDescriptor rpcEndpointDescriptor);

        bool IsHealth(IRpcEndpoint rpcEndpoint);

        void ChangeHealthStatus(IRpcEndpoint rpcEndpoint, bool isHealth, int unHealthCeilingTimes = 0);

        void ChangeHealthStatus(IPAddress mapToIPv4, int port, ServiceProtocol serviceProtocol, bool isHealth, int unHealthCeilingTimes = 0);

        void RemoveRpcEndpoint(IRpcEndpoint rpcEndpoint);
        
        void RemoveRpcEndpoint(IPAddress ipAddress, int port);

        event HealthChangeEvent OnHealthChange;
        event RemoveRpcEndpointEvent OnRemoveRpcEndpoint;
        event UnhealthEvent OnUnhealth;
        event AddMonitorEvent OnAddMonitor;
    }
}