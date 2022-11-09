using System;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using Silky.Core.Modularity.PlugIns;

namespace Silky.Core.Modularity
{
    public interface IModuleLoader
    {
        [NotNull]
        ISilkyModuleDescriptor[] LoadModules(
            [NotNull]IServiceCollection services,
            [NotNull]Type startupModuleType,
            [NotNull]PlugInSourceList plugInSources);
    }
}