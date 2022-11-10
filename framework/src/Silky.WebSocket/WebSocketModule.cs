using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Autofac;
using Silky.Core;
using Silky.Core.Modularity;
using Silky.Rpc;
using Silky.Rpc.Configuration;
using Silky.Rpc.Routing;
using Silky.Rpc.Runtime.Server;
using Silky.Rpc.Utils;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Silky.Core.Reflection;
using Silky.Core.Runtime.Rpc;
using Silky.Rpc.Endpoint;
using WebSocketSharp.Server;

namespace Silky.WebSocket
{
    [DependsOn(typeof(RpcModule))]
    public class WebSocketModule : SilkyModule
    {
        protected override void RegisterServices(ContainerBuilder builder)
        {
            var localWsEntryTypes = ServiceHelper.FindServiceLocalWsTypes(EngineContext.Current.TypeFinder)
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
                    .InstancePerLifetimeScope()
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
            var wsAddressModel = SilkyEndpointHelper.GetEndpoint(webSocketOptions.Port, ServiceProtocol.Ws);
            WebSocketServer socketServer = null;
            if (webSocketOptions.IsSsl)
            {
                socketServer = new WebSocketServer(IPAddress.Parse(wsAddressModel.Host), wsAddressModel.Port, true);
                socketServer.SslConfiguration.ServerCertificate = new X509Certificate2(
                    Path.Combine(hostEnvironment.ContentRootPath, webSocketOptions.SslCertificateName),
                    webSocketOptions.SslCertificatePassword);
            }
            else
            {
                socketServer = new WebSocketServer(IPAddress.Parse(wsAddressModel.Host), wsAddressModel.Port);
            }

            socketServer.KeepClean = webSocketOptions.KeepClean;
            socketServer.WaitTime = TimeSpan.FromSeconds(webSocketOptions.WaitTime);
            return socketServer;
        }

        public override async Task Initialize(ApplicationInitializationContext context)
        {
            var typeFinder = context.ServiceProvider.GetRequiredService<ITypeFinder>();
            var webSocketServices = GetWebSocketServices(typeFinder);
            var webSocketServerBootstrap =
                context.ServiceProvider.GetRequiredService<WebSocketServerBootstrap>();
            webSocketServerBootstrap.Initialize(webSocketServices);
            var serverProvider =
                context.ServiceProvider.GetRequiredService<IServerProvider>();

            serverProvider.AddWsServices();
        }

        private (Type, string)[] GetWebSocketServices(ITypeFinder typeFinder)
        {
            var wsServicesTypes = ServiceHelper.FindServiceLocalWsTypes(typeFinder);
            return wsServicesTypes.Select(p => (p, WebSocketResolverHelper.ParseWsPath(p))).ToArray();
        }
    }
}