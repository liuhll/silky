using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Silky.Core.Modularity
{
    public class ModuleManager : IModuleManager
    {
        private readonly IModuleContainer _moduleContainer;
        private readonly IServiceProvider _serviceProvider;
        private readonly IHostEnvironment _hostEnvironment;
        public ILogger<ModuleManager> Logger { get; set; }

        public ModuleManager(IModuleContainer moduleContainer,
            IServiceProvider serviceProvider,
            IHostEnvironment hostEnvironment)
        {
            _moduleContainer = moduleContainer;
            _serviceProvider = serviceProvider;
            _hostEnvironment = hostEnvironment;
            Logger = NullLogger<ModuleManager>.Instance;
        }

        public async Task PreInitializeModules()
        {
            await using var scope = _serviceProvider.CreateAsyncScope();
            foreach (var module in _moduleContainer.Modules)
            {
                try
                {
                    Logger.LogDebug("PreInitialize the module {0}", module.Name);
                    await module.Instance.PreInitialize(
                        new ApplicationInitializationContext(scope.ServiceProvider, _hostEnvironment));
                }
                catch (Exception e)
                {
                    Logger.LogError($"PreInitializing the {module.Name} module is an error, reason: {e.Message}");
                    throw;
                }
            }
            Logger.LogInformation("PreInitialize all Silky modules.");
        }

        public async Task InitializeModules()
        {
            await using var scope = _serviceProvider.CreateAsyncScope();
            foreach (var module in _moduleContainer.Modules)
            {
                try
                {
                    Logger.LogDebug("Initialize the module {0}", module.Name);
                    await module.Instance.Initialize(
                        new ApplicationInitializationContext(scope.ServiceProvider, _hostEnvironment));
                }
                catch (Exception e)
                {
                    Logger.LogError($"Initializing the {module.Name} module is an error, reason: {e.Message}");
                    throw;
                }
            }
            Logger.LogInformation("Initialize all Silky modules.");
        }

        public async Task PostInitializeModules()
        {
            await using var scope = _serviceProvider.CreateAsyncScope();
            foreach (var module in _moduleContainer.Modules)
            {
                try
                {
                    Logger.LogDebug("PostInitialize the module {0}", module.Name);
                    await module.Instance.PostInitialize(
                        new ApplicationInitializationContext(scope.ServiceProvider, _hostEnvironment));
                }
                catch (Exception e)
                {
                    Logger.LogError($"PostInitializing the {module.Name} module is an error, reason: {e.Message}");
                    throw;
                }
            }

            Logger.LogInformation("PostInitialize all Silky modules.");
        }

        public async Task ShutdownModules()
        {
            await using var scope = _serviceProvider.CreateAsyncScope();
            foreach (var module in _moduleContainer.Modules)
            {
                try
                {
                    Logger.LogDebug("Shutdown the module {0}", module.Name);
                    await module.Instance.Shutdown(new ApplicationShutdownContext(scope.ServiceProvider,
                        _hostEnvironment));
                }
                catch (Exception e)
                {
                    Logger.LogWarning($"Shutdown the {module.Name} module is an error, reason: {e.Message}");
                }
            }
        }
    }
}