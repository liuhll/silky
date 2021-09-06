using System.Collections.Generic;
using Silky.Rpc.AppServices.Dtos;
using Silky.Rpc.Runtime.Client;
using Silky.Rpc.Runtime.Server;
using Silky.Rpc.Runtime.Server.ServiceDiscovery;

namespace Silky.Rpc.AppServices
{
    [ServiceRoute]
    public interface IRpcAppService
    {
        [Governance(ProhibitExtranet = true)]
        GetInstanceDetailOutput GetInstanceDetail();
       
        [Governance(ProhibitExtranet = true)]
        IReadOnlyCollection<ServiceEntryHandleInfo> GetServiceEntryHandleInfos();
        
        [Governance(ProhibitExtranet = true)]
        IReadOnlyCollection<ServiceEntryInvokeInfo> GetServiceEntryInvokeInfos();
        
    }
}