using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Lms.Core.Exceptions;
using Lms.Core.Modularity;
using Lms.Rpc.Configuration;
using Lms.Rpc.Routing;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace Lms.Rpc
{
    public class RpcServerHostedService : IHostedService
    {
        private readonly RegistryCenterOptions _registryCenterOptions;
        private readonly IModuleContainer _moduleContainer;
        private readonly IServiceRouteProvider _serviceRouteProvider;

        public RpcServerHostedService(IOptions<RegistryCenterOptions> registryCenterOptions, 
            IModuleContainer moduleContainer, 
            IServiceRouteProvider serviceRouteProvider)
        {
            _moduleContainer = moduleContainer;
            _serviceRouteProvider = serviceRouteProvider;
            _registryCenterOptions = registryCenterOptions.Value;
        }


        public async Task StartAsync(CancellationToken cancellationToken)
        {
            if (!_moduleContainer.Modules.Any(p=> p.Name.Equals(_registryCenterOptions.RegistryCenterType.ToString(),StringComparison.OrdinalIgnoreCase)))
            {
                throw new LmsException($"您没有指定依赖{_registryCenterOptions.RegistryCenterType}服务注册中心模块");
            }
            await _serviceRouteProvider.RegisterRpcRoutes(Process.GetCurrentProcess().TotalProcessorTime.TotalMilliseconds);
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            
        }
    }
}