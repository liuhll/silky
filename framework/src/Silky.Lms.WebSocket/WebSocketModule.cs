using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Autofac;
using Silky.Lms.Core;
using Silky.Lms.Core.Modularity;
using Silky.Lms.Rpc;
using Silky.Lms.Rpc.Address;
using Silky.Lms.Rpc.Configuration;
using Silky.Lms.Rpc.Routing;
using Silky.Lms.Rpc.Runtime.Server;
using Silky.Lms.Rpc.Utils;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using WebSocketSharp.Server;

namespace Silky.Lms.WebSocket
{
    [DependsOn(typeof(RpcModule))]
    public class WebSocketModule : LmsModule
    {
        protected override void RegisterServices(ContainerBuilder builder)
        {
            var localWsEntryTypes = ServiceEntryHelper.FindServiceLocalWsEntryTypes(EngineContext.Current.TypeFinder)
                .ToArray();
            builder.RegisterTypes(localWsEntryTypes)
                .PropertiesAutowired()
                .AsSelf()
                .AsImplementedInterfaces();

            var serviceKeyTypes =
                localWsEntryTypes.Where(p => p.GetCustomAttributes().OfType<ServiceKeyAttribute>().Any());
            foreach (var serviceKeyType in serviceKeyTypes)
            {
                var serviceKeyAttribute = serviceKeyType.GetCustomAttributes().OfType<ServiceKeyAttribute>().First();
                builder.RegisterType(serviceKeyType).Named(serviceKeyAttribute.Name,
                        serviceKeyType.GetInterfaces().First(p =>
                            p.GetCustomAttributes().OfType<IRouteTemplateProvider>().Any()))
                    .InstancePerDependency()
                    .AsSelf()
                    .AsImplementedInterfaces();
            }

            builder.Register(CreateWebSocketServer)
                .AsSelf()
                .PropertiesAutowired()
                .SingleInstance();
        }

        private WebSocketServer CreateWebSocketServer(IComponentContext privider)
        {
            var webSocketOptions = privider.Resolve<IOptions<WebSocketOptions>>().Value;
            var hostEnvironment = privider.Resolve<IHostEnvironment>();
            var wsAddressModel = NetUtil.GetAddressModel(webSocketOptions.WsPort, ServiceProtocol.Ws);
            WebSocketServer socketServer = null;
            if (webSocketOptions.IsSsl)
            {
                socketServer = new WebSocketServer(IPAddress.Parse(wsAddressModel.Address), wsAddressModel.Port, true);
                socketServer.SslConfiguration.ServerCertificate = new X509Certificate2(
                    Path.Combine(hostEnvironment.ContentRootPath, webSocketOptions.SslCertificateName),
                    webSocketOptions.SslCertificatePassword);
            }
            else
            {
                socketServer = new WebSocketServer(IPAddress.Parse(wsAddressModel.Address), wsAddressModel.Port);
            }

            socketServer.KeepClean = webSocketOptions.KeepClean;
            socketServer.WaitTime = TimeSpan.FromSeconds(webSocketOptions.WaitTime);
            return socketServer;
        }

        public override async Task Initialize(ApplicationContext applicationContext)
        {
            var typeFinder = applicationContext.ServiceProvider.GetRequiredService<ITypeFinder>();
            var webSocketServices = GetWebSocketServices(typeFinder);
            var webSocketServerBootstrap =
                applicationContext.ServiceProvider.GetRequiredService<WebSocketServerBootstrap>();
            webSocketServerBootstrap.Initialize(webSocketServices);
            var serviceRouteProvder =
                applicationContext.ServiceProvider.GetRequiredService<IServiceRouteProvider>();
            await serviceRouteProvder.RegisterRpcRoutes(
                Process.GetCurrentProcess().TotalProcessorTime.TotalMilliseconds,
                ServiceProtocol.Ws);
            var webSocketOptions = applicationContext.ServiceProvider
                .GetRequiredService<IOptions<WebSocketOptions>>().Value;
            await serviceRouteProvder.RegisterWsRoutes(
                Process.GetCurrentProcess().TotalProcessorTime.TotalMilliseconds,
                webSocketServices.Select(p => p.Item1).ToArray(), webSocketOptions.WsPort);
        }

        private (Type, string)[] GetWebSocketServices(ITypeFinder typeFinder)
        {
            var wsServicesTypes = ServiceEntryHelper.FindServiceLocalWsEntryTypes(typeFinder);
            return wsServicesTypes.Select(p => (p, WebSocketResolverHelper.ParseWsPath(p))).ToArray();
        }
    }
}