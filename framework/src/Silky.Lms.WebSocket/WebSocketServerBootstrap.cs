using System;
using System.Linq;
using System.Reflection;
using Silky.Lms.Core;
using Silky.Lms.Core.DependencyInjection;
using Silky.Lms.Rpc.Runtime.Server;
using Microsoft.Extensions.Logging;
using WebSocketSharp.Server;

namespace Silky.Lms.WebSocket
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
                _logger.LogInformation($"Ws服务启动成功,服务地址:{_socketServer.Address}");
            }
            catch (Exception e)
            {
                _logger.LogError($"Ws服务启动失败,服务地址:{_socketServer.Address},原因：{e.Message}", e);
                throw;
            }
        }

        public void Dispose()
        {
            _socketServer?.Stop();
        }
    }
}