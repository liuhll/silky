using System;
using System.Collections.Generic;
using Autofac;
using Lms.Core.Modularity;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Lms.Core
{
    public interface IEngine
    {
        ITypeFinder TypeFinder { get; }
        
        void ConfigureServices(IServiceCollection services, IConfiguration configuration);
        
        void ConfigureRequestPipeline(IApplicationBuilder application);
        
        T Resolve<T>() where T : class;
        
        object Resolve(Type type);
        
        IEnumerable<T> ResolveAll<T>();
        
        bool IsRegistered(Type type);
        
        object ResolveUnregistered(Type type);
        
        void RegisterDependencies(ContainerBuilder builder);
        
        void RegisterModules<T>(IServiceCollection services, ContainerBuilder containerBuilder) where T : ILmsModule;
        
    }
}