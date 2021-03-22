using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Autofac;
using Lms.Core;
using Lms.Core.Modularity;
using Lms.Rpc;
using Lms.Rpc.Address;
using Lms.Rpc.Routing;
using Lms.Rpc.Runtime.Server;
using Lms.Rpc.Utils;
using Lms.WebSocket.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Lms.WebSocket
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
                .SingleInstance()
                .AsImplementedInterfaces();

            var serviceKeyTypes =
                localWsEntryTypes.Where(p => p.GetCustomAttributes().OfType<ServiceKeyAttribute>().Any());
            foreach (var serviceKeyType in serviceKeyTypes)
            {
                var serviceKeyAttribute = serviceKeyType.GetCustomAttributes().OfType<ServiceKeyAttribute>().First();
                builder.RegisterType(serviceKeyType).Named(serviceKeyAttribute.Name,
                        serviceKeyType.GetInterfaces().First(p =>
                            p.GetCustomAttributes().OfType<IRouteTemplateProvider>().Any()))
                    .PropertiesAutowired()
                    .AsSelf()
                    .SingleInstance()
                    .AsImplementedInterfaces();
            }
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