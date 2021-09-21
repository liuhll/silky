using System.Threading.Tasks;

namespace Silky.Rpc.Routing
{
    public interface IServerRouteProvider
    {
        Task EnterRoutes();
    }
}