using Silky.Rpc.AppServices.Dtos;
using Silky.Rpc.Runtime.Server;
using Silky.Rpc.Runtime.Server.ServiceDiscovery;

namespace Silky.Rpc.AppServices
{
    [ServiceRoute]
    public interface IRpcAppService
    {
        [Governance(ProhibitExtranet = true)]
        GetInstanceSupervisorOutput GetInstanceSupervisor();
       
        [Governance(ProhibitExtranet = true)]
        GetServiceEntrySupervisorOutput GetServiceEntrySupervisor(string serviceId);
    }
}