using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Silky.Rpc.Address;
using Silky.Rpc.Endpoint;
using Silky.Rpc.Runtime.Server;
using Silky.Rpc.Utils;

namespace Silky.Rpc.Routing
{
    public class DefaultServiceRouteRegisterProvider : IServiceRouteRegisterProvider
    {
        private readonly IServiceRouteRegister _serviceRouteRegister;
        public ILogger<DefaultServiceRouteRegisterProvider> Logger { get; set; }

        public DefaultServiceRouteRegisterProvider(IServiceRouteRegister serviceRouteRegister)
        {
            _serviceRouteRegister = serviceRouteRegister;
            Logger = NullLogger<DefaultServiceRouteRegisterProvider>.Instance;
        }

        public async Task RegisterTcpRoutes()
        {
            var hostAddress = AddressHelper.GetRpcEndpoint();
            await _serviceRouteRegister.RegisterRpcRoutes(hostAddress.Descriptor, ServiceProtocol.Tcp);
        }

        public async Task RegisterHttpRoutes()
        {
            var webAddressDescriptor = AddressHelper.GetLocalWebEndpointDescriptor();
            await _serviceRouteRegister.RegisterRpcRoutes(webAddressDescriptor, ServiceProtocol.Http);
        }

        public async Task RegisterWsRoutes(int wsPort)
        {
            var hostAddress = AddressHelper.GetRpcEndpoint(wsPort, ServiceProtocol.Ws);
            await _serviceRouteRegister.RegisterRpcRoutes(hostAddress.Descriptor, ServiceProtocol.Ws);
        }
    }
}