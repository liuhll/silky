using System;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;

namespace Silky.Core.Modularity.PlugIns;

public static class PlugInSourceExtensions
{
    [NotNull]
    public static Type[] GetModulesWithAllDependencies([NotNull] this IPlugInSource plugInSource, ILogger logger)
    {
        Check.NotNull(plugInSource, nameof(plugInSource));

        return plugInSource
            .GetModules()
            .SelectMany(type => SilkyModuleHelper.FindAllModuleTypes(type, logger))
            .Distinct()
            .ToArray();
    }
}