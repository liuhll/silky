using System.Collections.Generic;
using Silky.Core.DependencyInjection;
using Silky.Rpc.Address;
using Silky.Rpc.Configuration;
using Silky.Rpc.Endpoint;

namespace Silky.Rpc.Runtime.Client
{
    public interface IRequestServiceSupervisor : ISingletonDependency
    {
        void Monitor((string, IRpcEndpoint) item);

        void ExecSuccess((string, IRpcEndpoint) item, double elapsedTotalMilliseconds);

        void ExecFail((string, IRpcEndpoint) item, double elapsedTotalMilliseconds);

        ServiceInstanceInvokeInfo GetServiceInstanceInvokeInfo();

        IReadOnlyCollection<ServiceEntryInvokeInfo> GetServiceEntryInvokeInfos();
    }
}