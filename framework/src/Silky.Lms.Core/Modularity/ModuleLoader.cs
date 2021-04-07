using System;
using System.Collections.Generic;
using System.Linq;
using Silky.Lms.Core.DependencyInjection;
using Silky.Lms.Core.Extensions.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Silky.Lms.Core.Exceptions;

namespace Silky.Lms.Core.Modularity
{
    public class ModuleLoader : IModuleLoader
    {
        public ILmsModuleDescriptor[] LoadModules(IServiceCollection services, Type startupModuleType)
        {
            Check.NotNull(services, nameof(services));
            Check.NotNull(startupModuleType, nameof(startupModuleType));
            var modules = GetDescriptors(services, startupModuleType);
            
            modules = SortByDependency(modules, startupModuleType);
            return modules.ToArray();
        }
        
        private List<ILmsModuleDescriptor> SortByDependency(List<ILmsModuleDescriptor> modules, Type startupModuleType)
        {
            var sortedModules = modules.SortByDependencies(m => m.Dependencies);
            sortedModules.MoveItem(m => m.Type == startupModuleType, modules.Count - 1);
            return sortedModules;
        }

        private List<ILmsModuleDescriptor> GetDescriptors(IServiceCollection services, Type startupModuleType)
        {
            var modules = new List<LmsModuleDescriptor>();
            FillModules(modules, services, startupModuleType);
            SetDependencies(modules);
            
            return modules.Cast<ILmsModuleDescriptor>().ToList();
        }

        private void SetDependencies(List<LmsModuleDescriptor> modules)
        {
            foreach (var module in modules)
            {
                SetDependencies(modules, module);
            }
        }

        protected virtual void SetDependencies(List<LmsModuleDescriptor> modules, LmsModuleDescriptor module)
        {
            foreach (var dependedModuleType in LmsModuleHelper.FindDependedModuleTypes(module.Type))
            {
                var dependedModule = modules.FirstOrDefault(m => m.Type == dependedModuleType);
                if (dependedModule == null)
                {
                    throw new LmsException("Could not find a depended module " + dependedModuleType.AssemblyQualifiedName + " for " + module.Type.AssemblyQualifiedName);
                }

                module.AddDependency(dependedModule);
            }
        }

        private void FillModules(List<LmsModuleDescriptor> modules, IServiceCollection services, Type startupModuleType)
        {
            foreach (var moduleType in LmsModuleHelper.FindAllModuleTypes(startupModuleType))
            {
                modules.Add(CreateModuleDescriptor(services, moduleType));
            }
        }

        private LmsModuleDescriptor CreateModuleDescriptor(IServiceCollection services, Type moduleType)
        {
            return new LmsModuleDescriptor(moduleType, CreateAndRegisterModule(services, moduleType));
        }

        private ILmsModule CreateAndRegisterModule(IServiceCollection services, Type moduleType)
        {
            var module = (ILmsModule)Activator.CreateInstance(moduleType);
            services.AddSingleton(moduleType, module);
            return module;
        }
    }
}