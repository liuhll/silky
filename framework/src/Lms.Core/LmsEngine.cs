using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Lms.Core.DependencyInjection;
using Lms.Core.Exceptions;
using Lms.Core.Modularity;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Lms.Core
{
    public class LmsEngine : IEngine, IModuleContainer
    {
        private ITypeFinder _typeFinder;
        
        public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            _typeFinder = new AppDomainTypeFinder();
            ServiceProvider = services.BuildServiceProvider();    
            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
        }

        public ITypeFinder TypeFinder => _typeFinder;

        public void ConfigureRequestPipeline(IApplicationBuilder application)
        {
            ServiceProvider = application.ApplicationServices;
            
        }

        public T Resolve<T>() where T : class
        {
            return (T)Resolve(typeof(T));
        }

        public object Resolve(Type type)
        {
            var sp = GetServiceProvider();
            if (sp == null)
                return null;
            return sp.GetService(type);
        }

        public IEnumerable<T> ResolveAll<T>()
        {
            return (IEnumerable<T>)GetServiceProvider().GetServices(typeof(T));
        }

        public bool IsRegistered(Type type)
        {
            return GetServiceProvider().GetAutofacRoot().IsRegistered(type);
        }

        public object ResolveUnregistered(Type type)
        {
            Exception innerException = null;
            foreach (var constructor in type.GetConstructors())
            {
                try
                {
                    //try to resolve constructor parameters
                    var parameters = constructor.GetParameters().Select(parameter =>
                    {
                        var service = Resolve(parameter.ParameterType);
                        if (service == null)
                            throw new LmsException("Unknown dependency");
                        return service;
                    });

                    //all is ok, so create instance
                    return Activator.CreateInstance(type, parameters.ToArray());
                }
                catch (Exception ex)
                {
                    innerException = ex;
                }
            }

            throw new LmsException("No constructor was found that had all the dependencies satisfied.", innerException);
        }
        
        protected IServiceProvider GetServiceProvider()
        {
            if (ServiceProvider == null)
                return null;
            var accessor = ServiceProvider?.GetService<IHttpContextAccessor>();
            var context = accessor?.HttpContext;
            return context?.RequestServices ?? ServiceProvider;
        }

        public void RegisterDependencies(ContainerBuilder containerBuilder)
        {
            containerBuilder.RegisterInstance(this).As<IEngine>().SingleInstance();
            containerBuilder.RegisterInstance(_typeFinder).As<ITypeFinder>().SingleInstance();
            
            var dependencyRegistrars = _typeFinder.FindClassesOfType<IDependencyRegistrar>();
            var instances = dependencyRegistrars
                .Select(dependencyRegistrar => (IDependencyRegistrar)Activator.CreateInstance(dependencyRegistrar))
                .OrderBy(dependencyRegistrar => dependencyRegistrar.Order);
            foreach (var dependencyRegistrar in instances)
                dependencyRegistrar.Register(containerBuilder, _typeFinder);
        }

        private Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            //check for assembly already loaded
            var assembly = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(a => a.FullName == args.Name);
            if (assembly != null)
                return assembly;

            //get assembly from TypeFinder
            var tf = Resolve<ITypeFinder>();
            if (tf == null)
                return null;
            assembly = tf.GetAssemblies().FirstOrDefault(a => a.FullName == args.Name);
            return assembly;
        }
        
        public void RegisterModules<T>(IServiceCollection services, ContainerBuilder containerBuilder) where T : ILmsModule
        {
            var moduleLoader = services.GetSingletonInstance<IModuleLoader>(); 
            Modules = moduleLoader.LoadModules(services, typeof(T));
            containerBuilder.RegisterInstance(this).As<IModuleContainer>().SingleInstance();
            foreach (var module in Modules)
            {
                containerBuilder.RegisterModule((LmsModule)module.Instance);
            }
        }
        
        public virtual IServiceProvider ServiceProvider { get; set; }
        
        public IReadOnlyList<ILmsModuleDescriptor> Modules { get; protected set; }
    }
}