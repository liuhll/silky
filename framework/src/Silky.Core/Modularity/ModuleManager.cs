using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Silky.Core.Modularity
{
    public class ModuleManager : IModuleManager
    {
        private readonly IModuleContainer _moduleContainer;
        private readonly IServiceProvider _serviceProvider;
        public ILogger<ModuleManager> Logger { get; set; }

        public ModuleManager(IModuleContainer moduleContainer,
            IServiceProvider serviceProvider)
        {
            _moduleContainer = moduleContainer;
            _serviceProvider = serviceProvider;
            Logger = NullLogger<ModuleManager>.Instance;
        }

        public async Task InitializeModules()
        {
            using var scope = _serviceProvider.CreateScope();
            foreach (var module in _moduleContainer.Modules)
            {
                try
                {
                    Logger.LogInformation("Initialize the module {0}", module.Name);
                    await module.Instance.Initialize(
                        new ApplicationContext(scope.ServiceProvider, _moduleContainer));
                }
                catch (Exception e)
                {
                    Logger.LogError($"Initializing the {module.Name} module is an error, reason: {e.Message}");
                    throw;
                }
            }

            Logger.LogInformation("Initialized all Silky modules.");
        }

        public async Task ShutdownModules()
        {
            using var scope = _serviceProvider.CreateScope();
            foreach (var module in _moduleContainer.Modules)
            {
                try
                {
                    Logger.LogInformation("Shutdown the module {0}", module.Name);
                    await module.Instance.Shutdown(new ApplicationContext(scope.ServiceProvider, _moduleContainer));
                }
                catch (Exception e)
                {
                    Logger.LogWarning($"Shutdown the {module.Name} module is an error, reason: {e.Message}");
                }
            }
            Logger.LogInformation("Shutdown all Silky modules.");
        }
    }
}