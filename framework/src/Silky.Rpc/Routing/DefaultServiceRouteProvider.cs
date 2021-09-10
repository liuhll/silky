using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Silky.Rpc.Runtime.Server;
using Silky.Rpc.Utils;

namespace Silky.Rpc.Routing
{
    public class DefaultServiceRouteProvider : IServiceRouteProvider
    {
        private readonly IServiceRouteManager _serviceRouteManager;
        public ILogger<DefaultServiceRouteProvider> Logger { get; set; }

        public DefaultServiceRouteProvider(IServiceRouteManager serviceRouteManager)
        {
            _serviceRouteManager = serviceRouteManager;
            Logger = NullLogger<DefaultServiceRouteProvider>.Instance;
        }

        public async Task RegisterRpcRoutes(ServiceProtocol serviceProtocol)
        {
            var hostAddress = NetUtil.GetRpcAddressModel();
            await _serviceRouteManager.RegisterRpcRoutes(hostAddress.Descriptor, serviceProtocol);
        }

        public async Task RegisterWsRoutes(int wsPort)
        {
            var hostAddress = NetUtil.GetAddressModel(wsPort, ServiceProtocol.Ws);
            await _serviceRouteManager.RegisterRpcRoutes(hostAddress.Descriptor, ServiceProtocol.Ws);
        }
    }
}