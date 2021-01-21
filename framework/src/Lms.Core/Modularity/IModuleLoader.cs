using System;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;

namespace Lms.Core.Modularity
{
    public interface IModuleLoader
    {
        [NotNull]
        ILmsModuleDescriptor[] LoadModules(
            [NotNull] IServiceCollection services,
            [NotNull] Type startupModuleType
        );
    }
}