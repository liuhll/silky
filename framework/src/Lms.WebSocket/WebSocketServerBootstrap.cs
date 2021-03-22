using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using Lms.Core;
using Lms.Core.DependencyInjection;
using Lms.Rpc.Address;
using Lms.Rpc.Runtime.Server;
using Lms.Rpc.Utils;
using Lms.WebSocket.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using WebSocketSharp.Server;

namespace Lms.WebSocket
{
    internal class WebSocketServerBootstrap : IDisposable, ISingletonDependency
    {
        private WebSocketServer _socketServer;
        private readonly WebSocketOptions _webSocketOptions;
        private readonly IHostEnvironment _hostEnvironment;
        private readonly ILogger<WebSocketServerBootstrap> _logger;
        private readonly IAddressModel wsAddressModel;

        public WebSocketServerBootstrap(
            IOptions<WebSocketOptions> webSocketOptions,
            IHostEnvironment hostEnvironment,
            ILogger<WebSocketServerBootstrap> logger)
        {
            _hostEnvironment = hostEnvironment;
            _logger = logger;
            _webSocketOptions = webSocketOptions.Value;
            wsAddressModel = NetUtil.GetAddressModel(_webSocketOptions.WsPort, ServiceProtocol.Ws);
            if (_webSocketOptions.IsSsl)
            {
                _socketServer = new WebSocketServer(IPAddress.Parse(wsAddressModel.Address), wsAddressModel.Port, true);
                _socketServer.SslConfiguration.ServerCertificate = new X509Certificate2(
                    Path.Combine(_hostEnvironment.ContentRootPath, _webSocketOptions.SslCertificateName),
                    _webSocketOptions.SslCertificatePassword);
            }
            else
            {
                _socketServer = new WebSocketServer(IPAddress.Parse(wsAddressModel.Address), wsAddressModel.Port);
            }

            _socketServer.KeepClean = _webSocketOptions.KeepClean;
            _socketServer.WaitTime = TimeSpan.FromSeconds(_webSocketOptions.WaitTime);
            //_socketServer.AllowForwardedRequest = true;  
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
                       () => EngineContext.Current.ResolveNamed(serviceName, webSocketService.Item1) as WebSocketBehavior);
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
                _logger.LogInformation($"Ws服务启动成功,服务地址:{wsAddressModel.Address}");
            }
            catch (Exception e)
            {
                _logger.LogError($"Ws服务启动失败,服务地址:{wsAddressModel.Address},原因：{e.Message}", e);
                throw;
            }
        }

        public void Dispose()
        {
            _socketServer?.Stop();
        }
    }
}