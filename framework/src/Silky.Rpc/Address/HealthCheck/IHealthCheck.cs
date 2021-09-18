using System.Net;
using Silky.Core.DependencyInjection;
using Silky.Rpc.Address.Descriptor;
using Silky.Rpc.Routing;

namespace Silky.Rpc.Address.HealthCheck
{
    public interface IHealthCheck : ISingletonDependency
    {
        void Monitor(IRpcAddress rpcAddress);

        bool IsHealth(IPEndPoint ipEndPoint);

        bool IsHealth(AddressDescriptor addressDescriptor);

        bool IsHealth(IRpcAddress rpcAddress);

        void ChangeHealthStatus(IRpcAddress rpcAddress, bool isHealth, int unHealthCeilingTimes = 0);

        void ChangeHealthStatus(IPAddress mapToIPv4, int port, bool isHealth, int unHealthCeilingTimes = 0);

        void RemoveAddress(IRpcAddress rpcAddress);
        
        void RemoveAddress(IPAddress ipAddress, int port);

        event HealthChangeEvent OnHealthChange;
        event RemoveAddressEvent OnRemveAddress;
        event UnhealthEvent OnUnhealth;
        event AddMonitorEvent OnAddMonitor;
    }
}