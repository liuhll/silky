using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Silky.Core;
using Silky.Rpc.Gateway;
using Silky.Rpc.Runtime.Server;

namespace Silky.Rpc.Routing
{
    public class DefaultServiceRouteProvider : IServiceRouteProvider
    {
        private readonly IServiceRouteManager _serviceRouteManager;
        private readonly IGatewayManager _gatewayManager;
        public ILogger<DefaultServiceRouteProvider> Logger { get; set; }

        public DefaultServiceRouteProvider(IServiceRouteManager serviceRouteManager,
            IGatewayManager gatewayManager)
        {
            _serviceRouteManager = serviceRouteManager;
            _gatewayManager = gatewayManager;
            Logger = NullLogger<DefaultServiceRouteProvider>.Instance;
        }

        public async Task RegisterRpcRoutes(double processorTime, ServiceProtocol serviceProtocol)
        {
            await _serviceRouteManager.RegisterRpcRoutes(processorTime, serviceProtocol);
        }

        public async Task RegisterWsRoutes(double processorTime, Type[] wsAppServiceTypes, int wsPort)
        {
            await _serviceRouteManager.RegisterWsRoutes(processorTime, wsAppServiceTypes, wsPort);
        }
        
    }
}