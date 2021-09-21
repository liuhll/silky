using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Silky.Core;
using Silky.Core.Rpc;
using Silky.Rpc.Endpoint;
using Silky.Rpc.Runtime.Server;

namespace Silky.Rpc.Routing
{
    public class DefaultServerRegisterProvider : IServerRegisterProvider
    {
        public ILogger<DefaultServerRegisterProvider> Logger { get; set; }
        private readonly ServerRoute _serverRoute;
        private readonly IServiceManager _serviceManager;

        public DefaultServerRegisterProvider(IServiceManager serviceManager)
        {
            _serviceManager = serviceManager;
            Logger = NullLogger<DefaultServerRegisterProvider>.Instance;
            _serverRoute = new ServerRoute(EngineContext.Current.HostName);
        }

        public void AddTcpServices()
        {
            var rpcEndpoint = RpcEndpointHelper.GetLocalTcpEndpoint();
            _serverRoute.Endpoints.Add(rpcEndpoint);
            var tcpServices = _serviceManager.GetLocalService(ServiceProtocol.Tcp);
            foreach (var tcpService in tcpServices)
            {
                _serverRoute.Services.Add(tcpService.ServiceDescriptor);
            }
        }

        public void AddHttpServices()
        {
            var webEndpoint = RpcEndpointHelper.GetLocalWebEndpoint();
            _serverRoute.Endpoints.Add(webEndpoint);
            var httpServices = _serviceManager.GetLocalService(ServiceProtocol.Http);
            foreach (var httpService in httpServices)
            {
                _serverRoute.Services.Add(httpService.ServiceDescriptor);
            }
        }

        public void AddWsServices()
        {
            var wsEndpoint = RpcEndpointHelper.GetWsEndpoint();
            _serverRoute.Endpoints.Add(wsEndpoint);
            var wsServices = _serviceManager.GetLocalService(ServiceProtocol.Ws);
            foreach (var wsService in wsServices)
            {
                _serverRoute.Services.Add(wsService.ServiceDescriptor);
            }
        }

        public ServerRoute GetServerRoute()
        {
            return _serverRoute;
        }
    }
}