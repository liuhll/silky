using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Silky.Core.DependencyInjection;
using Silky.Core.Extensions.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Silky.Core.Exceptions;
using Silky.Core.Modularity.PlugIns;

namespace Silky.Core.Modularity
{
    public class ModuleLoader : IModuleLoader
    {
        public ISilkyModuleDescriptor[] LoadModules(
            [NotNull] IServiceCollection services,
            [NotNull] Type startupModuleType,
            [NotNull] PlugInSourceList plugInSources)
        {
            Check.NotNull(services, nameof(services));
            Check.NotNull(startupModuleType, nameof(startupModuleType));
            Check.NotNull(plugInSources, nameof(plugInSources));

            var modules = GetDescriptors(services, startupModuleType, plugInSources);
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

        private List<ISilkyModuleDescriptor> GetDescriptors(
            IServiceCollection services,
            Type startupModuleType,
            PlugInSourceList plugInSources)
        {
            var modules = new List<SilkyModuleDescriptor>();
            FillModules(modules, services, startupModuleType, plugInSources);
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

        private void FillModules(List<SilkyModuleDescriptor> modules,
            IServiceCollection services,
            Type startupModuleType,
            PlugInSourceList plugInSources)
        {
            var logger = services.GetInitLogger<SilkyEngine>();

            //All modules starting from the startup module
            foreach (var moduleType in SilkyModuleHelper.FindAllModuleTypes(startupModuleType, logger))
            {
                modules.Add(CreateModuleDescriptor(services, moduleType));
            }

            //Plugin modules
            foreach (var moduleType in plugInSources.GetAllModules(logger))
            {
                if (modules.Any(m => m.Type == moduleType))
                {
                    continue;
                }

                modules.Add(CreateModuleDescriptor(services, moduleType, isLoadedAsPlugIn: true));
            }
        }

        private SilkyModuleDescriptor CreateModuleDescriptor(IServiceCollection services, Type moduleType,
            bool isLoadedAsPlugIn = false)
        {
            return new SilkyModuleDescriptor(moduleType, CreateAndRegisterModule(services, moduleType),
                isLoadedAsPlugIn);
        }

        private ISilkyModule CreateAndRegisterModule(IServiceCollection services, Type moduleType)
        {
            var module = (ISilkyModule)Activator.CreateInstance(moduleType);
            services.AddSingleton(moduleType, module);
            return module;
        }
    }
}