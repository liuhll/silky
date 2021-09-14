using System.Threading.Tasks;
using Silky.Rpc.Address.Descriptor;
using Silky.Rpc.Runtime.Server;

namespace Silky.Rpc.Routing
{
    public interface IServiceRouteManager
    {
        Task RegisterRpcRoutes(AddressDescriptor addressDescriptor, ServiceProtocol serviceProtocol);

        Task EnterRoutes();
    }
}