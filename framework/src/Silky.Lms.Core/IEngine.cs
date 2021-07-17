using System;
using System.Collections.Generic;
using Autofac;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Silky.Lms.Core.Modularity;

namespace Silky.Lms.Core
{
    public interface IEngine
    {
        IServiceProvider ServiceProvider { get; set; }

        ITypeFinder TypeFinder { get; }

        IConfiguration Configuration { get; }
        IHostEnvironment HostEnvironment { get; }

        void ConfigureServices(IServiceCollection services, IConfiguration configuration,
            IHostEnvironment hostEnvironment);

        TOptions GetOptions<TOptions>()
            where TOptions : class, new();

        TOptions GetOptionsMonitor<TOptions>()
            where TOptions : class, new();

        TOptions GetOptionsSnapshot<TOptions>()
            where TOptions : class, new();
        
        
        
        void ConfigureRequestPipeline(IApplicationBuilder application);

        T Resolve<T>() where T : class;

        object Resolve(Type type);
        object ResolveNamed(string name, Type type);

        object ResolveServiceEntryInstance(string serviceKey, Type serviceType);

        T ResolveNamed<T>(string name);

        IEnumerable<T> ResolveAll<T>();

        bool IsRegistered(Type type);

        bool IsRegisteredWithName(string name, Type type);

        object ResolveUnregistered(Type type);

        void RegisterDependencies(ContainerBuilder builder);

        void RegisterModules<T>(IServiceCollection services, ContainerBuilder containerBuilder) where T : ILmsModule;
    }
}