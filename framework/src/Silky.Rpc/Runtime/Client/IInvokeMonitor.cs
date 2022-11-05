using System.Collections.Generic;
using System.Threading.Tasks;
using Silky.Rpc.Endpoint;

namespace Silky.Rpc.Runtime.Client
{
    public interface IInvokeMonitor
    {
        ClientInvokeInfo Monitor((string, ISilkyEndpoint) item);

        void ExecSuccess((string, ISilkyEndpoint) item, double elapsedTotalMilliseconds,
            ClientInvokeInfo clientInvokeInfo);

        void ExecFail((string, ISilkyEndpoint) item, double elapsedTotalMilliseconds, ClientInvokeInfo clientInvokeInfo);

        Task ClearCache();

        Task<ServerInstanceInvokeInfo> GetServerInstanceInvokeInfo();

        Task<IReadOnlyCollection<ClientInvokeInfo>> GetServiceEntryInvokeInfos();
    }
}