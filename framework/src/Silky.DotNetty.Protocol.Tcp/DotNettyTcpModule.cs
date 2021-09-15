using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Autofac;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Silky.Core;
using Silky.Core.Exceptions;
using Silky.Core.Modularity;
using Silky.Rpc;
using Silky.Rpc.Configuration;
using Silky.Rpc.Extensions;
using Silky.Rpc.Routing;
using Silky.Rpc.Runtime.Server;

namespace Silky.DotNetty.Protocol.Tcp
{
    [DependsOn(typeof(RpcModule), typeof(DotNettyModule))]
    public class DotNettyTcpModule : SilkyModule
    {
        protected override void RegisterServices(ContainerBuilder builder)
        {
            builder.RegisterType<DotNettyTcpServerMessageListener>()
                .AsSelf()
                .SingleInstance()
                .AsImplementedInterfaces()
                .PropertiesAutowired();
        }

        public override async Task Initialize(ApplicationContext applicationContext)
        {
            Check.NotNull(applicationContext, nameof(applicationContext));
            if (!applicationContext.IsDependsOnRegistryCenterModule(out var registryCenterType))
            {
                throw new SilkyException(
                    $"You did not specify the dependent {registryCenterType} service registry module");
            }

            var messageListener = applicationContext.ServiceProvider.GetRequiredService<DotNettyTcpServerMessageListener>();
            await messageListener.Listen();
            var serviceRouteRegisterProvider = applicationContext.ServiceProvider.GetRequiredService<IServiceRouteRegisterProvider>();
            await serviceRouteRegisterProvider.RegisterTcpRoutes();
        }
    }
}