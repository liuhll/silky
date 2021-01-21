using System;
using System.Collections.Generic;
using System.Linq;
using Autofac;
using Lms.Core.Configuration;
using Lms.Core.DependencyInjection;
using Lms.Core.Modularity;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Lms.Core
{
    public class LmsEngine : IEngine
    {
        private ITypeFinder _typeFinder;

        public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            _typeFinder = new AppDomainTypeFinder();
        }

        public void ConfigureRequestPipeline(IApplicationBuilder application)
        {
            
            throw new System.NotImplementedException();
        }

        public T Resolve<T>() where T : class
        {
            throw new NotImplementedException();
        }

        public object Resolve(Type type)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<T> ResolveAll<T>()
        {
            throw new NotImplementedException();
        }

        public object ResolveUnregistered(Type type)
        {
            throw new NotImplementedException();
        }

        public void RegisterDependencies(ContainerBuilder containerBuilder, AppSettings appSettings)
        {
            containerBuilder.RegisterInstance(this).As<IEngine>().SingleInstance();
            containerBuilder.RegisterInstance(_typeFinder).As<ITypeFinder>().SingleInstance();
            
            var dependencyRegistrars = _typeFinder.FindClassesOfType<IDependencyRegistrar>();
            var instances = dependencyRegistrars
                .Select(dependencyRegistrar => (IDependencyRegistrar)Activator.CreateInstance(dependencyRegistrar))
                .OrderBy(dependencyRegistrar => dependencyRegistrar.Order);
            foreach (var dependencyRegistrar in instances)
                dependencyRegistrar.Register(containerBuilder, _typeFinder, appSettings);
        }

        public void RegisterModules<T>(IServiceCollection services, ContainerBuilder containerBuilder) where T : ILmsModule
        {
            var moduleLoader = services.GetSingletonInstance<IModuleLoader>();
            var modules = moduleLoader.LoadModules(services, typeof(T));
            foreach (var module in modules)
            {
                containerBuilder.RegisterModule((LmsModule)module.Instance);
            }
        }
    }
}