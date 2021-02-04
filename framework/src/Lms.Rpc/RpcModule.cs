using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Autofac;
using Lms.Core;
using Lms.Core.Exceptions;
using Lms.Core.Modularity;
using Lms.Rpc.Configuration;
using Lms.Rpc.Routing;
using Lms.Rpc.Runtime.Server.ServiceEntry;
using Microsoft.Extensions.Options;

namespace Lms.Rpc
{
    public class RpcModule : LmsModule
    {
        protected override void RegisterServices(ContainerBuilder builder)
        {
            builder.RegisterTypes(
                ServiceEntryHelper.FindServiceLocalEntryTypes(EngineContext.Current.TypeFinder).ToArray())
                .AsSelf()
                .AsImplementedInterfaces();
        }

        public async override Task Initialize(ApplicationContext applicationContext)
        {
            var registryCenterOptions = EngineContext.Current.Resolve<IOptions<RegistryCenterOptions>>().Value;
            if (!applicationContext.ModuleContainer.Modules.Any(p=> p.Name.Equals(registryCenterOptions.RegistryCenterType.ToString(),StringComparison.OrdinalIgnoreCase)))
            {
                throw new LmsException($"您没有指定依赖{registryCenterOptions.RegistryCenterType}服务注册中心模块");
            }

            var serviceRouteProvider = EngineContext.Current.Resolve<IServiceRouteProvider>();
            await serviceRouteProvider.RegisterRpcRoutes(Process.GetCurrentProcess().TotalProcessorTime.TotalMilliseconds);
            
            // cache
            var cache = EngineContext.Current.Resolve<ServiceRouteCache>();
            var c = cache.ServiceRoutes;
        }
    }
}