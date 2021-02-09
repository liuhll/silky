using System.Threading.Tasks;

namespace Lms.Rpc.Address
{
    public delegate Task HealthChangeEvent(IAddressModel addressModel, bool isHealth);

    public delegate Task UnhealthEvent(IAddressModel addressModel);
    
    public delegate Task RemoveAddressEvent(IAddressModel addressModel);

}