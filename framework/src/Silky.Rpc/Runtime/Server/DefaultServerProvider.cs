using System.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Silky.Core;
using Silky.Core.Exceptions;
using Silky.Core.Runtime.Rpc;
using Silky.Rpc.Endpoint;

namespace Silky.Rpc.Runtime.Server
{
    public class DefaultServerProvider : IServerProvider
    {
        public ILogger<DefaultServerProvider> Logger { get; set; }
        private readonly IServer _server;
        private readonly IServiceManager _serviceManager;
        private readonly IServiceEntryManager _serviceEntryManager;

        public DefaultServerProvider(IServiceManager serviceManager,
            IServiceEntryManager serviceEntryManager)
        {
            _serviceManager = serviceManager;
            _serviceEntryManager = serviceEntryManager;
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
            if (_serviceEntryManager.HasHttpProtocolServiceEntry() && !_server.Endpoints.Any(p =>
                    p.ServiceProtocol == ServiceProtocol.Http || p.ServiceProtocol == ServiceProtocol.Https))
            {
                throw new SilkyException("A server that supports file upload and download or ActionResult must be built through the http protocol host", StatusCode.ServerError);
            }

            return _server;
        }
    }
}