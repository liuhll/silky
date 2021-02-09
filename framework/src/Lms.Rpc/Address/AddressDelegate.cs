using System.Threading.Tasks;

namespace Lms.Rpc.Address
{
    public delegate Task HealthChange(IAddressModel addressModel, bool isHealth);

    public delegate Task ReachUnHealthCeilingTimes(IAddressModel addressModel);

}