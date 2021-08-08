using System;
using System.Threading.Tasks;
using Silky.Rpc.Address;
using Silky.Rpc.Configuration;
using Silky.Rpc.Runtime.Server;

namespace Silky.Rpc.Routing
{
    public interface IServiceRouteManager
    {
        // Task SetRoutesAsync(IReadOnlyList<ServiceRouteDescriptor> serviceRouteDescriptors);

        Task CreateSubscribeDataChanges();

        void UpdateRegistryCenterOptions(RegistryCenterOptions options);
        
        Task CreateWsSubscribeDataChanges(string[] wsPaths);

        Task RegisterRpcRoutes(double processorTime, ServiceProtocol serviceProtocol);

        Task RegisterWsRoutes(double processorTime, Type[] wsAppServiceTypes, int wsPort);

        Task EnterRoutes();
        
        Task RemoveServiceRoute(string serviceId, IAddressModel selectedAddress);
    }
}