using System;
using System.Linq;
using System.Reflection;
using Silky.Core;
using Silky.Core.DependencyInjection;
using Silky.Rpc.Runtime.Server;
using Microsoft.Extensions.Logging;
using Silky.Core.Logging;
using WebSocketSharp.Server;

namespace Silky.WebSocket
{
    internal class WebSocketServerBootstrap : IDisposable, ISingletonDependency
    {
        private WebSocketServer _socketServer;
        private readonly ILogger<WebSocketServerBootstrap> _logger;

        public WebSocketServerBootstrap(
            ILogger<WebSocketServerBootstrap> logger, WebSocketServer socketServer)
        {
            _logger = logger;
            _socketServer = socketServer;
        }

        public void Initialize((Type, string)[] webSocketServices)
        {
            foreach (var webSocketService in webSocketServices)
            {
                var serviceKeyAttribute = webSocketService.Item1.GetCustomAttributes().OfType<ServiceKeyAttribute>()
                    .FirstOrDefault();

                if (serviceKeyAttribute != null)
                {
                    var serviceName = serviceKeyAttribute.Name;
                    _socketServer.AddWebSocketService(webSocketService.Item2,
                        () =>
                            EngineContext.Current.ResolveNamed(serviceName,
                                webSocketService.Item1) as WebSocketBehavior);
                }
                else
                {
                    _socketServer.AddWebSocketService(webSocketService.Item2,
                        () => EngineContext.Current.Resolve(webSocketService.Item1) as WebSocketBehavior);
                }
            }

            try
            {
                _socketServer.Start();
                _logger.LogInformation(
                    "Now Silky WebSocket Server listening on: {0}{1}:{2}",_socketServer.IsSecure ? "wss://" : "ws://", _socketServer.Address,
                    _socketServer.Port.ToString());
            }
            catch (Exception ex)
            {
                _logger.LogException(ex);
                throw;
            }
        }

        public void Dispose()
        {
            _socketServer?.Stop();
        }
    }
}