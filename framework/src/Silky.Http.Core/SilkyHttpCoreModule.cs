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
using Silky.Http.Core.Handlers;
using Silky.Rpc.Gateway;

namespace Silky.Http.Core
{
    [DependsOn(typeof(RpcModule), typeof(RpcProxyModule))]
    public class SilkyHttpCoreModule : WebSilkyModule
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

        
        public override async Task Initialize(ApplicationContext applicationContext)
        {
            var registryCenterOptions =
                applicationContext.ServiceProvider.GetRequiredService<IOptions<RegistryCenterOptions>>().Value;
            if (!applicationContext.ModuleContainer.Modules.Any(p =>
                p.Name.Equals(registryCenterOptions.RegistryCenterType.ToString(), StringComparison.OrdinalIgnoreCase)))
            {
                throw new SilkyException(
                    $"You did not specify the dependent {registryCenterOptions.RegistryCenterType} service registry module");
            }

            var gatewayManager = applicationContext.ServiceProvider.GetRequiredService<IGatewayManager>();
            await gatewayManager.CreateSubscribeGatewayDataChanges();
            await gatewayManager.EnterGateways();
        }
    }
}