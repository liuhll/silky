using System;
using System.Linq;
using System.Threading.Tasks;
using Autofac;
using Silky.Core.Exceptions;
using Silky.Core.Modularity;
using Silky.Rpc;
using Silky.Rpc.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Silky.Http.Core.Handlers;

namespace Silky.Http.Core
{
    [DependsOn(typeof(RpcModule))]
    public class SilkyHttpCoreModule : HttpSilkyModule
    {
        protected override void RegisterServices(ContainerBuilder builder)
        {
            builder.RegisterType<OuterHttpMessageReceivedHandler>()
                .InstancePerLifetimeScope()
                .AsSelf()
                .Named<IMessageReceivedHandler>(HttpMessageType.Outer.ToString());
            builder.RegisterType<OuterHttpMessageReceivedHandler>()
                .InstancePerLifetimeScope()
                .AsSelf()
                .Named<IMessageReceivedHandler>(HttpMessageType.Inner.ToString());
            
            
            builder.RegisterType<OuterHttpRequestParameterParser>()
                .InstancePerLifetimeScope()
                .AsSelf()
                .Named<IParameterParser>(HttpMessageType.Outer.ToString());
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
        }
    }
}