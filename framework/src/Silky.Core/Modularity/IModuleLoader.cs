using System;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;

namespace Silky.Core.Modularity
{
    public interface IModuleLoader
    {
        [NotNull]
        ISilkyModuleDescriptor[] LoadModules(
            [NotNull] IServiceCollection services,
            [NotNull] Type startupModuleType
        );
    }
}