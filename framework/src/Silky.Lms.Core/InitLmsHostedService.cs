using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Silky.Lms.Core.Modularity;

namespace Silky.Lms.Core
{
    public class InitLmsHostedService : IHostedService
    {
        private readonly IModuleManager _moduleManager;

        public InitLmsHostedService(IServiceProvider serviceProvider,
            IModuleManager moduleManager)
        {
            if (EngineContext.Current is LmsEngine)
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