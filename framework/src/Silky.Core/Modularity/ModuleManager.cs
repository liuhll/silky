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
 
        private readonly IHostEnvironment _hostEnvironment;
        public ILogger<ModuleManager> Logger { get; set; }

        public ModuleManager(IModuleContainer moduleContainer,
            IHostEnvironment hostEnvironment)
        {
            _moduleContainer = moduleContainer;

            _hostEnvironment = hostEnvironment;
            Logger = NullLogger<ModuleManager>.Instance;
        }

        public async Task PreInitializeModules(IServiceProvider serviceProvider)
        {
            foreach (var module in _moduleContainer.Modules)
            {
                try
                {
                    Logger.LogDebug("PreInitialize the module {0}", module.Name);
                    await module.Instance.PreInitialize(
                        new ApplicationInitializationContext(serviceProvider, _hostEnvironment));
                }
                catch (Exception e)
                {
                    Logger.LogError($"PreInitializing the {module.Name} module is an error, reason: {e.Message}");
                    throw;
                }
            }

            Logger.LogInformation("PreInitialize all Silky modules.");
        }

        public async Task InitializeModules(IServiceProvider serviceProvider)
        {
            foreach (var module in _moduleContainer.Modules)
            {
                try
                {
                    Logger.LogDebug("Initialize the module {0}", module.Name);
                    await module.Instance.Initialize(
                        new ApplicationInitializationContext(serviceProvider, _hostEnvironment));
                }
                catch (Exception e)
                {
                    Logger.LogError($"Initializing the {module.Name} module is an error, reason: {e.Message}");
                    throw;
                }
            }

            Logger.LogInformation("Initialize all Silky modules.");
        }

        public async Task PostInitializeModules(IServiceProvider serviceProvider)
        {
            foreach (var module in _moduleContainer.Modules)
            {
                try
                {
                    Logger.LogDebug("PostInitialize the module {0}", module.Name);
                    await module.Instance.PostInitialize(
                        new ApplicationInitializationContext(serviceProvider, _hostEnvironment));
                }
                catch (Exception e)
                {
                    Logger.LogError($"PostInitializing the {module.Name} module is an error, reason: {e.Message}");
                    throw;
                }
            }

            Logger.LogInformation("PostInitialize all Silky modules.");
        }

        public async Task ShutdownModules(IServiceProvider serviceProvider)
        {
            foreach (var module in _moduleContainer.Modules)
            {
                try
                {
                    Logger.LogDebug("Shutdown the module {0}", module.Name);
                    await module.Instance.Shutdown(new ApplicationShutdownContext(serviceProvider,
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