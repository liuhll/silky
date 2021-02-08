using System.Threading.Tasks;
using Lms.Core.DependencyInjection;
using Lms.Rpc.Runtime.Server;

namespace Lms.Rpc.Routing
{
    public interface IServiceRouteProvider : ISingletonDependency
    {
        Task RegisterRpcRoutes(double processorTime, ServiceProtocol serviceProtocol);
    }
}