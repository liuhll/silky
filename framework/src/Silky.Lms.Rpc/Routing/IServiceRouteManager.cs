using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Silky.Lms.Rpc.Runtime.Server;

namespace Silky.Lms.Rpc.Routing
{
    public interface IServiceRouteManager
    {
        // Task SetRoutesAsync(IReadOnlyList<ServiceRouteDescriptor> serviceRouteDescriptors);

        Task CreateSubscribeDataChanges();

        Task CreateWsSubscribeDataChanges(string[] wsPaths);

        Task RegisterRpcRoutes(double processorTime, ServiceProtocol serviceProtocol);

        Task RegisterWsRoutes(double processorTime, Type[] wsAppServiceTypes, int wsPort);

        Task EnterRoutes();
    }
}