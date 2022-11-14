using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Silky.Core.Modularity;
using Silky.Core.Utils;

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
            
            var bannerPrinter = EngineContext.Current.Resolve<IBannerPrinter>();
            bannerPrinter.Print();

            await _moduleManager.PreInitializeModules();
            _hostApplicationLifetime.ApplicationStarted.Register(async () =>
            {
                await _moduleManager.InitializeModules();
                await _moduleManager.PostInitializeModules();
                _logger.LogInformation("Initialized all Silky modules.");
            });
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            _hostApplicationLifetime.ApplicationStopping.Register(async () =>
            {
                await _moduleManager.ShutdownModules();
                _logger.LogInformation("Shutdown all Silky modules.");
            });
        }
    }
}