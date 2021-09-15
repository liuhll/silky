using System.Threading.Tasks;

namespace Silky.Rpc.Routing
{
    public interface IServiceRouteProvider
    {
        Task EnterRoutes();
    }
}