using System;
using System.Linq;
using System.Threading.Tasks;
using Lms.Core.Exceptions;
using Lms.Core.Modularity;
using Lms.Rpc.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Lms.HttpServer
{
    public class HttpServerModule : LmsModule
    {
        public async override Task Initialize(ApplicationContext applicationContext)
        {
            var registryCenterOptions =
                applicationContext.ServiceProvider.GetService<IOptions<RegistryCenterOptions>>().Value;
            if (!applicationContext.ModuleContainer.Modules.Any(p=> p.Name.Equals(registryCenterOptions.RegistryCenterType.ToString(),StringComparison.OrdinalIgnoreCase)))
            {
                throw new LmsException($"您没有指定依赖的{registryCenterOptions.RegistryCenterType}服务注册中心模块");
            }
        }
    }
}