using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Silky.Lms.Core.Extensions.Collections.Generic;

namespace Silky.Lms.Core.Modularity
{
    internal static class LmsModuleHelper
    {
        public static List<Type> FindAllModuleTypes(Type startupModuleType)
        {
            var moduleTypes = new List<Type>();
            AddModuleAndDependenciesResursively(moduleTypes, startupModuleType);
            return moduleTypes;
        }

        private static void AddModuleAndDependenciesResursively(List<Type> moduleTypes, Type moduleType)
        {
            LmsModule.CheckLmsModuleType(moduleType);
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
            LmsModule.CheckLmsModuleType(moduleType);

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