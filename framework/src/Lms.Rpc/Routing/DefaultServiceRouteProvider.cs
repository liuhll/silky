using System.Threading.Tasks;
using Lms.Rpc.Runtime.Server.ServiceEntry;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Lms.Rpc.Routing
{
    public class DefaultServiceRouteProvider : IServiceRouteProvider
    {
        private readonly IServiceEntryManager _serviceEntryManager;
        private readonly IServiceRouteManager _serviceRouteManager;
        private readonly ILogger<DefaultServiceRouteProvider> _logger;

        public DefaultServiceRouteProvider(IServiceEntryManager serviceEntryManager, 
            IServiceRouteManager serviceRouteManager)
        {
            _serviceEntryManager = serviceEntryManager;
            _serviceRouteManager = serviceRouteManager;
            _logger = NullLogger<DefaultServiceRouteProvider>.Instance;
        }

        public Task RegisterRoutes(double processorTime)
        {
            throw new System.NotImplementedException();
        }
    }
}