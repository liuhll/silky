using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.Logging;
using Silky.Core.Extensions.Collections.Generic;

namespace Silky.Core.Modularity
{
    internal static class SilkyModuleHelper
    {
        public static List<Type> FindAllModuleTypes(Type startupModuleType, ILogger logger)
        {
            var moduleTypes = new List<Type>();
            logger.Log(LogLevel.Information, "Loaded Silky modules:");
            AddModuleAndDependenciesResursively(moduleTypes, startupModuleType);
            return moduleTypes;
        }

        private static void AddModuleAndDependenciesResursively(List<Type> moduleTypes, Type moduleType)
        {
            SilkyModule.CheckSilkyModuleType(moduleType);
            if (moduleTypes.Contains(moduleType))
            {
                return;
            }

            moduleTypes.Add(moduleType);
            foreach (var dependedModuleType in FindDependedModuleTypes(moduleType))
            {
                AddModuleAndDependenciesResursively(moduleTypes, dependedModuleType);
            }
        }

        public static List<Type> FindDependedModuleTypes(Type moduleType)
        {
            SilkyModule.CheckSilkyModuleType(moduleType);

            var dependencies = new List<Type>();

            var dependencyDescriptors = moduleType
                .GetCustomAttributes()
                .OfType<IDependedTypesProvider>();

            foreach (var descriptor in dependencyDescriptors)
            {
                foreach (var dependedModuleType in descriptor.GetDependedTypes())
                {
                    dependencies.AddIfNotContains(dependedModuleType);
                }
            }

            return dependencies;
        }
    }
}