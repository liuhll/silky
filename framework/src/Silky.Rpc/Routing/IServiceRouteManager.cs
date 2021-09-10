using System;
using System.Threading.Tasks;
using Silky.Rpc.Address;
using Silky.Rpc.Address.Descriptor;
using Silky.Rpc.Configuration;
using Silky.Rpc.Runtime.Server;

namespace Silky.Rpc.Routing
{
    public interface IServiceRouteManager
    {
        
        Task CreateSubscribeServiceRouteDataChanges();
        
        Task RegisterRpcRoutes(AddressDescriptor addressDescriptor, ServiceProtocol serviceProtocol);
        
        Task EnterRoutes();
        
        Task RemoveServiceRoute(string hostName, IAddressModel selectedAddress);
        
    }
}