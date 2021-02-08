using System.Threading.Tasks;
using Lms.Rpc.Runtime.Server;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Lms.Rpc.Routing
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

        public async Task RegisterRpcRoutes(double processorTime)
        {
            await _serviceRouteManager.RegisterRoutes(processorTime, ServiceProtocol.Tcp);
        }
    }
}