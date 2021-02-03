using System.Threading.Tasks;
using Lms.Rpc.Address;
using Lms.Rpc.Runtime.Server.ServiceEntry;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Lms.Rpc.Routing
{
    public class DefaultServiceRouteProvider : IServiceRouteProvider
    {
        private readonly IServiceRouteManager _serviceRouteManager;
        private readonly ILogger<DefaultServiceRouteProvider> _logger;

        public DefaultServiceRouteProvider(IServiceRouteManager serviceRouteManager)
        {

            _serviceRouteManager = serviceRouteManager;
            _logger = NullLogger<DefaultServiceRouteProvider>.Instance;
        }

        public async Task RegisterRpcRoutes(double processorTime)
        {
            await _serviceRouteManager.RegisterRoutes(processorTime, ServiceProtocol.Tcp);
        }
    }
}