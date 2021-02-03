using System.Threading.Tasks;
using Lms.Core.DependencyInjection;

namespace Lms.Rpc.Routing
{
    public interface IServiceRouteProvider : ISingletonDependency
    {
        Task RegisterRpcRoutes(double processorTime);
    }
}