using System.Threading.Tasks;

namespace Lms.Rpc.Routing
{
    public interface IServiceRouteProvider
    {
        Task RegisterRoutes(double processorTime);
    }
}