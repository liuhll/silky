using System;
using JetBrains.Annotations;

namespace Silky.Core.Modularity
{
    public class ApplicationContext
    {
        public IServiceProvider ServiceProvider { get; private set; }

        public IModuleContainer ModuleContainer { get; private set; }

        internal ApplicationContext([NotNull] IServiceProvider serviceProvider,
            [NotNull] IModuleContainer moduleContainer)
        {
            Check.NotNull(serviceProvider, nameof(serviceProvider));
            Check.NotNull(moduleContainer, nameof(moduleContainer));
            ServiceProvider = serviceProvider;
            ModuleContainer = moduleContainer;
        }
    }
}