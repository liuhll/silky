using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Silky.Core.Modularity;

namespace Silky.Core
{
    public class InitSilkyHostedService : IHostedService
    {
        private readonly IModuleManager _moduleManager;
        private readonly IHostApplicationLifetime _hostApplicationLifetime;
        private readonly ILogger<InitSilkyHostedService> _logger;

        public InitSilkyHostedService(IServiceProvider serviceProvider,
            IModuleManager moduleManager,
            IHostApplicationLifetime hostApplicationLifetime,
            ILogger<InitSilkyHostedService> logger)
        {
            if (EngineContext.Current is SilkyEngine)
            {
                EngineContext.Current.ServiceProvider = serviceProvider;
            }

            _moduleManager = moduleManager;
            _hostApplicationLifetime = hostApplicationLifetime;
            _logger = logger;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await _moduleManager.PreInitializeModules(EngineContext.Current.ServiceProvider);
            _hostApplicationLifetime.ApplicationStarted.Register(async () =>
            {
                
                await _moduleManager.InitializeModules(EngineContext.Current.ServiceProvider);
                await _moduleManager.PostInitializeModules(EngineContext.Current.ServiceProvider);
              
                _logger.LogInformation($"{EngineContext.Current.HostName} Host Started Successfully!");
            });
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            _hostApplicationLifetime.ApplicationStopped.Register(async () =>
            {
                await _moduleManager.ShutdownModules(EngineContext.Current.ServiceProvider);
                _logger.LogInformation("Shutdown all Silky modules.");
            });
        }
    }
}