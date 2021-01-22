using System.Threading;
using System.Threading.Tasks;
using Lms.Core.Modularity;
using Microsoft.Extensions.Hosting;

namespace Lms.Rpc
{
    public class RpcServerHostedService: IHostedService
    {
        private readonly IModuleManager _moduleManager;

        public RpcServerHostedService(IModuleManager moduleManager)
        {
            _moduleManager = moduleManager;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
        
            await _moduleManager.InitializeModules();
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
          
            await _moduleManager.ShutdownModules();
        }
    }
}