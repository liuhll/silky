using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Lms.Core.Modularity
{
    public class ModuleManager : IModuleManager
    {
        private readonly IModuleContainer _moduleContainer;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<ModuleManager> _logger;

        public ModuleManager(IModuleContainer moduleContainer,
            IServiceProvider serviceProvider)
        {
            _moduleContainer = moduleContainer;
            _serviceProvider = serviceProvider;
            _logger = NullLogger<ModuleManager>.Instance;
        }

        public async Task InitializeModules()
        {
            foreach (var module in _moduleContainer.Modules)
            {
                try
                {
                    _logger.LogInformation($"初始化模块{module.Name}");
                    await module.Instance.Initialize(new ApplicationContext(_serviceProvider, _moduleContainer));
                }
                catch (Exception e)
                {
                    _logger.LogError($"初始化{module.Name}模块是错误,原因:{e.Message}");
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