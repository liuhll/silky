using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Silky.Core.Rpc;
using Silky.Rpc.Endpoint;

namespace Silky.Rpc.Routing
{
    public class DefaultServerRegisterProvider : IServerRegisterProvider
    {
        private readonly IServerRouteRegister _serverRouteRegister;
        public ILogger<DefaultServerRegisterProvider> Logger { get; set; }

        public DefaultServerRegisterProvider(IServerRouteRegister serverRouteRegister)
        {
            _serverRouteRegister = serverRouteRegister;
            Logger = NullLogger<DefaultServerRegisterProvider>.Instance;
        }

        public async Task RegisterTcpRoutes()
        {
            var hostAddress = AddressHelper.GetRpcEndpoint();
            await _serverRouteRegister.RegisterRpcRoutes(hostAddress.Descriptor, ServiceProtocol.Tcp);
        }

        public async Task RegisterHttpRoutes()
        {
            var webAddressDescriptor = AddressHelper.GetLocalWebEndpointDescriptor();
            await _serverRouteRegister.RegisterRpcRoutes(webAddressDescriptor, ServiceProtocol.Http);
        }

        public async Task RegisterWsRoutes(int wsPort)
        {
            var hostAddress = AddressHelper.GetRpcEndpoint(wsPort, ServiceProtocol.Ws);
            await _serverRouteRegister.RegisterRpcRoutes(hostAddress.Descriptor, ServiceProtocol.Ws);
        }
    }
}