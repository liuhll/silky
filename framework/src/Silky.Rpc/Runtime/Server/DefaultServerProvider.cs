using System.Linq;
using Microsoft.Extensions.Logging;
using Silky.Core;
using Silky.Core.Exceptions;
using Silky.Core.Extensions.Collections.Generic;
using Silky.Core.Runtime.Rpc;
using Silky.Core.Serialization;
using Silky.Rpc.Endpoint;

namespace Silky.Rpc.Runtime.Server
{
    public class DefaultServerProvider : IServerProvider
    {
        public ILogger<DefaultServerProvider> Logger { get; set; }
        private readonly IServer _server;
        private readonly IServiceManager _serviceManager;
        private readonly ISerializer _serializer;

        public DefaultServerProvider(IServiceManager serviceManager,
            ISerializer serializer)
        {
            _serviceManager = serviceManager;
            _serializer = serializer;
            Logger = EngineContext.Current.Resolve<ILogger<DefaultServerProvider>>();
            _server = new Server(EngineContext.Current.HostName);
        }

        public void AddRpcServices()
        {
            var rpcEndpoint = SilkyEndpointHelper.GetLocalRpcEndpoint();
            _server.Endpoints.Add(rpcEndpoint);
            var rpcServices = _serviceManager.GetLocalService(ServiceProtocol.Rpc);
            foreach (var rpcService in rpcServices)
            {
                _server.Services.AddIfNotContains(rpcService.ServiceDescriptor);
            }
        }

        public void AddHttpServices()
        {
            var webEndpoint = SilkyEndpointHelper.GetLocalWebEndpoint();
            if (webEndpoint == null)
            {
                throw new SilkyException("Failed to obtain http service rpcEndpoint");
            }

            _server.Endpoints.AddIfNotContains(webEndpoint);
        }

        public void AddWsServices()
        {
            var wsEndpoint = SilkyEndpointHelper.GetWsEndpoint();
            _server.Endpoints.Add(wsEndpoint);
            var wsServices = _serviceManager.GetLocalService(ServiceProtocol.Ws);
            foreach (var wsService in wsServices)
            {
                _server.Services.AddIfNotContains(wsService.ServiceDescriptor);
            }
        }

        public IServer GetServer()
        {
            Logger.LogDebug($"{_server.HostName} server endpoints:" + _serializer.Serialize(_server.Endpoints.Select(p => p.ToString())));
            if (_server.HasHttpProtocolServiceEntry() && !_server.Endpoints.Any(p =>
                    p.ServiceProtocol == ServiceProtocol.Http || p.ServiceProtocol == ServiceProtocol.Https))
            {
                throw new SilkyException(
                    $"{_server.HostName} server that supports file upload and download or ActionResult must be built through the http protocol host",
                    StatusCode.ServerError);
            }
            return _server;
        }
    }
}