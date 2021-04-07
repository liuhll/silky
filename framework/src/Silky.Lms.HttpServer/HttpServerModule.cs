using System;
using System.Linq;
using System.Threading.Tasks;
using Autofac;
using Silky.Lms.Core.Exceptions;
using Silky.Lms.Core.Modularity;
using Silky.Lms.Rpc;
using Silky.Lms.Rpc.Configuration;
using Silky.Lms.Rpc.Proxy;
using Silky.Lms.Rpc.Runtime.Server;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Silky.Lms.HttpServer.Handlers;

namespace Silky.Lms.HttpServer
{
    [DependsOn(typeof(RpcModule), typeof(RpcProxyModule))]
    public class HttpServerModule : LmsModule
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
                throw new LmsException($"您没有指定依赖的{registryCenterOptions.RegistryCenterType}服务注册中心模块");
            }
        }
    }
}