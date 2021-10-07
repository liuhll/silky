using System.Collections.Generic;
using System.Threading.Tasks;
using Silky.Rpc.AppServices.Dtos;
using Silky.Rpc.Routing;
using Silky.Rpc.Runtime.Client;
using Silky.Rpc.Runtime.Server;

namespace Silky.Rpc.AppServices
{
    [ServiceRoute]
    [Metadata(ServiceConstant.IsSilkyService, true)]
    public interface IRpcAppService
    {
        [Governance(ProhibitExtranet = true)]
        Task<GetInstanceDetailOutput> GetInstanceDetail();

        [Governance(ProhibitExtranet = true)]
        Task<IReadOnlyCollection<ServerHandleInfo>>  GetServiceEntryHandleInfos();

        [Governance(ProhibitExtranet = true)]
        Task<IReadOnlyCollection<ClientInvokeInfo>> GetServiceEntryInvokeInfos();

        [Governance(ProhibitExtranet = true, TimeoutMillSeconds = 1000)]
        Task<bool> IsHealth();
    }
}