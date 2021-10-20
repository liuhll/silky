using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Silky.Core;
using Silky.Core.Rpc;
using Silky.Rpc.Endpoint;

namespace Silky.Rpc.Runtime.Server
{
    public class DefaultServerProvider : IServerProvider
    {
        public ILogger<DefaultServerProvider> Logger { get; set; }
        private readonly IServer _server;
        private readonly IServiceManager _serviceManager;

        public DefaultServerProvider(IServiceManager serviceManager)
        {
            _serviceManager = serviceManager;
            Logger = NullLogger<DefaultServerProvider>.Instance;
            _server = new Server(EngineContext.Current.HostName);
        }

        public void AddTcpServices()
        {
            var rpcEndpoint = RpcEndpointHelper.GetLocalTcpEndpoint();
            _server.Endpoints.Add(rpcEndpoint);
            var tcpServices = _serviceManager.GetLocalService(ServiceProtocol.Tcp);
            foreach (var tcpService in tcpServices)
            {
                _server.Services.Add(tcpService.ServiceDescriptor);
            }
        }

        public void AddHttpServices()
        {
            var webEndpoint = RpcEndpointHelper.GetLocalWebEndpoint();
            if (webEndpoint != null)
            {
                _server.Endpoints.Add(webEndpoint);
            }
            
        }

        public void AddWsServices()
        {
            var wsEndpoint = RpcEndpointHelper.GetWsEndpoint();
            _server.Endpoints.Add(wsEndpoint);
            var wsServices = _serviceManager.GetLocalService(ServiceProtocol.Ws);
            foreach (var wsService in wsServices)
            {
                _server.Services.Add(wsService.ServiceDescriptor);
            }
        }

        public IServer GetServer()
        {
            return _server;
        }
    }
}