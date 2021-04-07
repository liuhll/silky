using Silky.Lms.Core.DependencyInjection;
using Silky.Lms.Rpc.Address;
using Silky.Lms.Rpc.Configuration;

namespace Silky.Lms.Rpc.Runtime.Client
{
    public interface IRemoteServiceSupervisor : ISingletonDependency
    {
        void Monitor((string, IAddressModel) item, GovernanceOptions governanceOptions);

        void ExecSuccess((string, IAddressModel) item, double elapsedTotalMilliseconds);

        void ExceFail((string, IAddressModel) item, double elapsedTotalMilliseconds);
    }
}