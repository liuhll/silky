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

        Task<ServiceInstanceInvokeInfo> GetServiceInstanceInvokeInfo();

        Task<IReadOnlyCollection<ServiceEntryInvokeInfo>> GetServiceEntryInvokeInfos();
    }
}