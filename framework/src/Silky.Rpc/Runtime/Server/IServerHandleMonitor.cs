using System.Collections.Generic;
using System.Threading.Tasks;

namespace Silky.Rpc.Runtime.Server
{
    public interface IServerHandleMonitor
    {
        ServerHandleInfo Monitor((string, string) item);

        void ExecSuccess((string, string) item, double elapsedTotalMilliseconds, ServerHandleInfo serverHandleInfo);

        void ExecFail((string, string) item, bool isSeriousError, double elapsedTotalMilliseconds,
            ServerHandleInfo serverHandleInfo);

        Task ClearCache();

        Task<ServerInstanceHandleInfo> GetServerInstanceHandleInfo();

        Task<IReadOnlyCollection<ServerHandleInfo>> GetServiceEntryHandleInfos();
    }
}