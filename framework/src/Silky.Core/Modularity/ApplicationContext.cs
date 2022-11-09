using System;
using JetBrains.Annotations;
using Microsoft.Extensions.Hosting;

namespace Silky.Core.Modularity
{
    public class ApplicationContext
    {
        public IServiceProvider ServiceProvider { get; private set; }

        public IModuleContainer ModuleContainer { get; private set; }
        
        public IHostEnvironment HostEnvironment { get; private set; }

        internal ApplicationContext(
            [NotNull] IServiceProvider serviceProvider,
            [NotNull] IModuleContainer moduleContainer,
            [NotNull] IHostEnvironment hostEnvironment)
        {
            Check.NotNull(serviceProvider, nameof(serviceProvider));
            Check.NotNull(moduleContainer, nameof(moduleContainer));
            Check.NotNull(hostEnvironment, nameof(hostEnvironment));
            ServiceProvider = serviceProvider;
            ModuleContainer = moduleContainer;
            HostEnvironment = hostEnvironment;
        }
    }
}