using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Silky.Core.Modularity;

namespace Silky.Core
{
    public class InitSilkyHostedService : IHostedService
    {
        private readonly IModuleManager _moduleManager;

        public InitSilkyHostedService(IServiceProvider serviceProvider,
            IModuleManager moduleManager)
        {
            if (EngineContext.Current is SilkyEngine)
            {
                EngineContext.Current.ServiceProvider = serviceProvider;
            }

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