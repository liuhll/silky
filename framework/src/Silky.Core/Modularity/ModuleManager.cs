using System;
using System.Threading.Tasks;
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
            foreach (var module in _moduleContainer.Modules)
            {
                try
                {
                    Logger.LogInformation("Initialize the module {0}", module.Name);
                    await module.Instance.Initialize(new ApplicationContext(_serviceProvider, _moduleContainer));
                }
                catch (Exception e)
                {
                    Logger.LogError($"Initializing the {module.Name} module is an error, reason: {e.Message}");
                    throw;
                }
            }
        }

        public async Task ShutdownModules()
        {
            foreach (var module in _moduleContainer.Modules)
            {
                await module.Instance.Shutdown(new ApplicationContext(_serviceProvider, _moduleContainer));
            }
        }
    }
}