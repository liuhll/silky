using System;
using System.Collections.Generic;
using System.Linq;
using Silky.Core.DependencyInjection;
using Silky.Core.Extensions.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Silky.Core.Exceptions;

namespace Silky.Core.Modularity
{
    public class ModuleLoader : IModuleLoader
    {
        public ISilkyModuleDescriptor[] LoadModules(IServiceCollection services, Type startupModuleType)
        {
            Check.NotNull(services, nameof(services));
            Check.NotNull(startupModuleType, nameof(startupModuleType));
            var modules = GetDescriptors(services, startupModuleType);

            modules = SortByDependency(modules, startupModuleType);
            return modules.ToArray();
        }

        private List<ISilkyModuleDescriptor> SortByDependency(List<ISilkyModuleDescriptor> modules,
            Type startupModuleType)
        {
            var sortedModules = modules.SortByDependencies(m => m.Dependencies);
            sortedModules.MoveItem(m => m.Type == startupModuleType, modules.Count - 1);
            return sortedModules;
        }

        private List<ISilkyModuleDescriptor> GetDescriptors(IServiceCollection services, Type startupModuleType)
        {
            var modules = new List<SilkyModuleDescriptor>();
            FillModules(modules, services, startupModuleType);
            SetDependencies(modules);

            return modules.Cast<ISilkyModuleDescriptor>().ToList();
        }

        private void SetDependencies(List<SilkyModuleDescriptor> modules)
        {
            foreach (var module in modules)
            {
                SetDependencies(modules, module);
            }
        }

        protected virtual void SetDependencies(List<SilkyModuleDescriptor> modules, SilkyModuleDescriptor module)
        {
            foreach (var dependedModuleType in SilkyModuleHelper.FindDependedModuleTypes(module.Type))
            {
                var dependedModule = modules.FirstOrDefault(m => m.Type == dependedModuleType);
                if (dependedModule == null)
                {
                    throw new SilkyException("Could not find a depended module " +
                                             dependedModuleType.AssemblyQualifiedName + " for " +
                                             module.Type.AssemblyQualifiedName);
                }

                module.AddDependency(dependedModule);
            }
        }

        private void FillModules(List<SilkyModuleDescriptor> modules, IServiceCollection services,
            Type startupModuleType)
        {
            foreach (var moduleType in SilkyModuleHelper.FindAllModuleTypes(startupModuleType))
            {
                modules.Add(CreateModuleDescriptor(services, moduleType));
            }
        }

        private SilkyModuleDescriptor CreateModuleDescriptor(IServiceCollection services, Type moduleType)
        {
            return new SilkyModuleDescriptor(moduleType, CreateAndRegisterModule(services, moduleType));
        }

        private ISilkyModule CreateAndRegisterModule(IServiceCollection services, Type moduleType)
        {
            var module = (ISilkyModule)Activator.CreateInstance(moduleType);
            services.AddSingleton(moduleType, module);
            return module;
        }
    }
}