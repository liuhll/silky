using Silky.Core.DependencyInjection;
using Silky.Rpc.Address;
using Silky.Rpc.Configuration;

namespace Silky.Rpc.Runtime.Client
{
    public interface IRemoteServiceSupervisor : ISingletonDependency
    {
        void Monitor((string, IAddressModel) item, GovernanceOptions governanceOptions);

        void ExecSuccess((string, IAddressModel) item, double elapsedTotalMilliseconds);

        void ExceFail((string, IAddressModel) item, double elapsedTotalMilliseconds);
    }
}