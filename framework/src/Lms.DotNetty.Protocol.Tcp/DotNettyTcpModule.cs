using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Autofac;
using Lms.Core;
using Lms.Core.Exceptions;
using Lms.Core.Modularity;
using Lms.Rpc;
using Lms.Rpc.Configuration;
using Lms.Rpc.Routing;
using Lms.Rpc.Runtime.Server;
using Lms.Rpc.Transport;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Lms.DotNetty.Protocol.Tcp
{
    [DependsOn(typeof(RpcModule), typeof(DotNettyModule))]
    public class DotNettyTcpModule : LmsModule
    {
        protected override void RegisterServices(ContainerBuilder builder)
        {
            builder.RegisterType<DotNettyTcpServerMessageListener>()
                .AsSelf()
                .SingleInstance()
                .AsImplementedInterfaces()
                .PropertiesAutowired();
        }

        public async override Task Initialize(ApplicationContext applicationContext)
        {
            Check.NotNull(applicationContext, nameof(applicationContext));
            var registryCenterOptions =
                applicationContext.ServiceProvider.GetRequiredService<IOptions<RegistryCenterOptions>>().Value;
            if (!applicationContext.ModuleContainer.Modules.Any(p =>
                p.Name.Equals(registryCenterOptions.RegistryCenterType.ToString(), StringComparison.OrdinalIgnoreCase)))
            {
                throw new LmsException($"您没有指定依赖的{registryCenterOptions.RegistryCenterType}服务注册中心模块");
            }

            var messageListener = applicationContext.ServiceProvider.GetRequiredService<DotNettyTcpServerMessageListener>();
            await messageListener.Listen();
            var serviceRouteProvider = applicationContext.ServiceProvider.GetRequiredService<IServiceRouteProvider>();
            await serviceRouteProvider.RegisterRpcRoutes(
                Process.GetCurrentProcess().TotalProcessorTime.TotalMilliseconds, ServiceProtocol.Tcp);
        }
    }
}