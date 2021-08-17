using System.Threading.Tasks;
using Silky.Rpc.Address;

namespace Silky.Rpc.Gateway
{
    public interface IGatewayManager
    {
        Task RegisterGateway();
        Task CreateSubscribeGatewayDataChanges();
        Task EnterGateways();
        
        Task RemoveGatewayAddress(string hostName, IAddressModel selectedAddress);

        Task RemoveLocalGatewayAddress();
    }
}