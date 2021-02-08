using System.Threading.Tasks;
using Lms.Rpc.Runtime.Server;

namespace Lms.Rpc.Routing
{
    public interface IServiceRouteManager 
    {
        // Task SetRoutesAsync(IReadOnlyList<ServiceRouteDescriptor> serviceRouteDescriptors);
        
        Task RegisterRoutes(double processorTime, ServiceProtocol serviceProtocol);

        Task EnterRoutes();
    }
}