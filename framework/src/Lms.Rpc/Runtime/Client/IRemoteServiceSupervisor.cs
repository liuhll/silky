using Lms.Core.DependencyInjection;
using Lms.Rpc.Address;
using Lms.Rpc.Configuration;

namespace Lms.Rpc.Runtime.Client
{
    public interface IRemoteServiceSupervisor : ISingletonDependency
    {
        void Monitor((string, IAddressModel) item, GovernanceOptions governanceOptions);

        void ExecSuccess((string, IAddressModel) item, double elapsedTotalMilliseconds);

        void ExceFail((string, IAddressModel) item, double elapsedTotalMilliseconds);
    }
}