using System;
using Microsoft.Extensions.Logging;

namespace Lms.Core.Modularity
{
    public class ModuleManager : IModuleManager
    {
        private readonly IModuleContainer _moduleContainer;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<ModuleManager> _logger;

        public ModuleManager(IModuleContainer moduleContainer, 
            IServiceProvider serviceProvider, 
            ILogger<ModuleManager> logger)
        {
            _moduleContainer = moduleContainer;
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        public void InitializeModules()
        {
            foreach (var module in _moduleContainer.Modules)
            {
                _logger.LogInformation($"初始化模块{module.Name}");
                module.Instance.Initialize(new ApplicationContext(_serviceProvider));
            }
        }

        public void ShutdownModules()
        {
            foreach (var module in _moduleContainer.Modules)
            {
                module.Instance.Shutdown(new ApplicationContext(_serviceProvider));
            }
        }
    }
}