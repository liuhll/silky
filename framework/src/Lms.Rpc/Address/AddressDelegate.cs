using System.Threading.Tasks;

namespace Lms.Rpc.Address
{
    public delegate Task HealthChange(IAddressModel addressModel, bool isHealth);

    public delegate Task Unhealth(IAddressModel addressModel);
    
    public delegate Task ReachUnHealthCeilingTimes(IAddressModel addressModel);
    
    public delegate Task RemveAddress(IAddressModel addressModel);

}