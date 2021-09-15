using System.Threading.Tasks;

namespace Silky.Rpc.Routing
{
    public interface IServiceRouteManager : IServiceRouteRegister
    {
        Task EnterRoutes();
    }
}