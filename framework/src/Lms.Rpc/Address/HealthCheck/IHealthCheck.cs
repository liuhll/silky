using System.Net;
using Lms.Core.DependencyInjection;
using Lms.Rpc.Address.Descriptor;

namespace Lms.Rpc.Address.HealthCheck
{
    public interface IHealthCheck : ISingletonDependency
    {
        void Monitor(IAddressModel addressModel);

        bool IsHealth(IPEndPoint ipEndPoint);

        bool IsHealth(AddressDescriptor addressDescriptor);

        bool IsHealth(IAddressModel addressModel);

        event HealthChange HealthChange;

        event ReachUnHealthCeilingTimes ReachUnHealthCeilingTimes;


        void ChangeHealthStatus(IAddressModel addressModel, bool isHealth);
    }
}