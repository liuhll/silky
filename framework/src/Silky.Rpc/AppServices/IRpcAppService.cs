using System.Collections.Generic;
using System.Threading.Tasks;
using Silky.Rpc.AppServices.Dtos;
using Silky.Rpc.Routing;
using Silky.Rpc.Runtime.Client;
using Silky.Rpc.Runtime.Server;

namespace Silky.Rpc.AppServices
{
    [ServiceRoute(ServiceName = "Rpc")]
    [Metadata(ServiceConstant.IsSilkyService, true)]
    public interface IRpcAppService
    {
        [Governance(ProhibitExtranet = true)]
        GetInstanceDetailOutput GetInstanceDetail();

        [Governance(ProhibitExtranet = true)]
        IReadOnlyCollection<ServiceEntryHandleInfo> GetServiceEntryHandleInfos();

        [Governance(ProhibitExtranet = true)]
        IReadOnlyCollection<ServiceEntryInvokeInfo> GetServiceEntryInvokeInfos();

        [Governance(ProhibitExtranet = true, TimeoutMillSeconds = 1000)]
        Task<bool> IsHealth();
    }
}