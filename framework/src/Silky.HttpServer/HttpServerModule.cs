using System;
using System.Linq;
using System.Threading.Tasks;
using Autofac;
using Silky.Core.Exceptions;
using Silky.Core.Modularity;
using Silky.Rpc;
using Silky.Rpc.Configuration;
using Silky.Rpc.Proxy;
using Silky.Rpc.Runtime.Server;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Silky.HttpServer.Handlers;

namespace Silky.HttpServer
{
    [DependsOn(typeof(RpcModule), typeof(RpcProxyModule))]
    public class HttpServerModule : SilkyModule
    {
        protected override void RegisterServices(ContainerBuilder builder)
        {
            builder.RegisterType<HttpMessageReceivedHandler>()
                .InstancePerLifetimeScope()
                .AsSelf()
                .Named<IMessageReceivedHandler>(ServiceProtocol.Tcp.ToString());

            builder.RegisterType<WsMessageReceivedHandler>()
                .InstancePerLifetimeScope()
                .AsSelf()
                .Named<IMessageReceivedHandler>(ServiceProtocol.Ws.ToString());

            builder.RegisterType<MqttMessageReceivedHandler>()
                .InstancePerLifetimeScope()
                .AsSelf()
                .Named<IMessageReceivedHandler>(ServiceProtocol.Mqtt.ToString());
        }

        public async override Task Initialize(ApplicationContext applicationContext)
        {
            var registryCenterOptions =
                applicationContext.ServiceProvider.GetRequiredService<IOptions<RegistryCenterOptions>>().Value;
            if (!applicationContext.ModuleContainer.Modules.Any(p =>
                p.Name.Equals(registryCenterOptions.RegistryCenterType.ToString(), StringComparison.OrdinalIgnoreCase)))
            {
                throw new SilkyException($"您没有指定依赖的{registryCenterOptions.RegistryCenterType}服务注册中心模块");
            }
        }
    }
}