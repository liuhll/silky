using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Autofac;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Silky.Lms.Core;
using Silky.Lms.Core.Exceptions;
using Silky.Lms.Core.Modularity;
using Silky.Lms.Rpc;
using Silky.Lms.Rpc.Configuration;
using Silky.Lms.Rpc.Routing;
using Silky.Lms.Rpc.Runtime.Server;

namespace Silky.Lms.DotNetty.Protocol.Tcp
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