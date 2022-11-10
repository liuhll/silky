using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;

namespace Silky.Core.Modularity.PlugIns;

public class PlugInSourceList : List<IPlugInSource>
{
    [NotNull]
    internal Type[] GetAllModules(ILogger logger)
    {
        return this
            .SelectMany(pluginSource => pluginSource.GetModulesWithAllDependencies(logger))
            .Distinct()
            .ToArray();
    }
    
}