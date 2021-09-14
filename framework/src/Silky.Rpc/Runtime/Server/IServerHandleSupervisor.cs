using System.Collections.Generic;
using Silky.Core.DependencyInjection;

namespace Silky.Rpc.Runtime.Server
{
    public interface IServerHandleSupervisor : ISingletonDependency
    {
        void Monitor((string, string) item);

        void ExecSuccess((string, string) item, double elapsedTotalMilliseconds);

        void ExecFail((string, string) item, bool isSeriousError, double elapsedTotalMilliseconds);
        
        ServiceInstanceHandleInfo GetServiceInstanceHandleInfo();

        IReadOnlyCollection<ServiceEntryHandleInfo> GetServiceEntryHandleInfos();
        
    }
}