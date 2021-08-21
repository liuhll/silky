using System.Collections.Generic;
using Silky.Core.DependencyInjection;
using Silky.Rpc.Address;
using Silky.Rpc.Configuration;

namespace Silky.Rpc.Runtime.Client
{
    public interface IRequestServiceSupervisor : ISingletonDependency
    {
        void Monitor((string, IAddressModel) item, GovernanceOptions governanceOptions);

        void ExecSuccess((string, IAddressModel) item, double elapsedTotalMilliseconds);

        void ExecFail((string, IAddressModel) item, double elapsedTotalMilliseconds);

        ServiceInstanceInvokeInfo GetServiceInstanceInvokeInfo();

        IReadOnlyCollection<ServiceEntryInvokeInfo> GetServiceInvokeInfo(string serviceId);
    }
}