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

namespace Lms.DotNetty.Protocol.Ws
{
    [DependsOn(typeof(RpcModule), typeof(DotNettyModule))]
    public class DotNettyHttpModule : LmsModule
    {
        protected override void RegisterServices(ContainerBuilder builder)
        {
            builder.RegisterType<DotNettyWsServerMessageListener>()
                .AsSelf()
                .SingleInstance()
                .PropertiesAutowired();
               
        }
        
        public async override Task Initialize(ApplicationContext applicationContext)
        {
            Check.NotNull(applicationContext, nameof(applicationContext));
            var registryCenterOptions =
                applicationContext.ServiceProvider.GetService<IOptions<RegistryCenterOptions>>().Value;
            if (!applicationContext.ModuleContainer.Modules.Any(p =>
                p.Name.Equals(registryCenterOptions.RegistryCenterType.ToString(), StringComparison.OrdinalIgnoreCase)))
            {
                throw new LmsException($"您没有指定依赖的{registryCenterOptions.RegistryCenterType}服务注册中心模块");
            }

            var messageListener = applicationContext.ServiceProvider.GetService<DotNettyWsServerMessageListener>();
            await messageListener.Listen();
            var serviceRouteProvider = applicationContext.ServiceProvider.GetService<IServiceRouteProvider>();
            await serviceRouteProvider.RegisterRpcRoutes(Process.GetCurrentProcess().TotalProcessorTime.TotalMilliseconds, ServiceProtocol.Ws);
        }
    }
}