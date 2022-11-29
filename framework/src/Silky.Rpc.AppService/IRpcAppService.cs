using System.Collections.Generic;
using System.Threading.Tasks;
using Silky.Rpc.Routing;
using Silky.Rpc.Runtime.Client;
using Silky.Rpc.Runtime.Server;

namespace Silky.Rpc.AppService
{
    [ServiceRoute]
    [SilkyAppService]
    public interface IRpcAppService
    {
        [ProhibitExtranet]
        Task<ServerInstanceDetailInfo> GetInstanceDetail();

        [ProhibitExtranet]
        Task<IReadOnlyCollection<ServerHandleInfo>> GetServiceEntryHandleInfos();

        [ProhibitExtranet]
        Task<IReadOnlyCollection<ClientInvokeInfo>> GetServiceEntryInvokeInfos();

        [ProhibitExtranet]
        [Governance(TimeoutMillSeconds = 2000)]
        Task<bool> IsHealth();
    }
}