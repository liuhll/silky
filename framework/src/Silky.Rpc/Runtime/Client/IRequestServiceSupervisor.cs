using System.Collections.Generic;
using Silky.Core.DependencyInjection;
using Silky.Rpc.Address;
using Silky.Rpc.Configuration;

namespace Silky.Rpc.Runtime.Client
{
    public interface IRequestServiceSupervisor : ISingletonDependency
    {
        void Monitor((string, IRpcAddress) item);

        void ExecSuccess((string, IRpcAddress) item, double elapsedTotalMilliseconds);

        void ExecFail((string, IRpcAddress) item, double elapsedTotalMilliseconds);

        ServiceInstanceInvokeInfo GetServiceInstanceInvokeInfo();

        IReadOnlyCollection<ServiceEntryInvokeInfo> GetServiceEntryInvokeInfos();
    }
}