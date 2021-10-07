using System.Collections.Generic;
using System.Threading.Tasks;
using Silky.Rpc.Endpoint;

namespace Silky.Rpc.Runtime.Client
{
    public interface IInvokeMonitor
    {
        ClientInvokeInfo Monitor((string, IRpcEndpoint) item);

        void ExecSuccess((string, IRpcEndpoint) item, double elapsedTotalMilliseconds,
            ClientInvokeInfo clientInvokeInfo);

        void ExecFail((string, IRpcEndpoint) item, double elapsedTotalMilliseconds, ClientInvokeInfo clientInvokeInfo);

        Task ClearCache();

        Task<ServerInstanceInvokeInfo> GetServerInstanceInvokeInfo();

        Task<IReadOnlyCollection<ClientInvokeInfo>> GetServiceEntryInvokeInfos();
    }
}