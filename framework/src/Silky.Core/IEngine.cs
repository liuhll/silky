using System;
using System.Collections.Generic;
using Autofac;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Silky.Core.Configuration;
using Silky.Core.Modularity;
using Silky.Core.Modularity.PlugIns;
using Silky.Core.Reflection;

namespace Silky.Core
{
    public interface IEngine
    {
        SilkyApplicationCreationOptions ApplicationOptions { get; }
        
        IServiceProvider ServiceProvider { get; set; }

        ITypeFinder TypeFinder { get; }

        IConfiguration Configuration { get; }

        IHostEnvironment HostEnvironment { get; }
        
        internal  Banner Banner { get; set; }

        string HostName { get; }
        
        internal void SetTypeFinder([NotNull] IServiceCollection services, [NotNull] ISilkyFileProvider fileProvider,
            AppServicePlugInSourceList appServicePlugInSources, PlugInSourceList modulePlugInSources);

        internal void SetHostEnvironment([NotNull] IHostEnvironment hostEnvironment);

        internal void SetConfiguration([NotNull] IConfiguration configuration);

        internal void ConfigureServices([NotNull] IServiceCollection services, [NotNull] IConfiguration configuration);

        internal void LoadModules(IServiceCollection services, Type startUpType, IModuleLoader moduleLoader,
            [NotNull] PlugInSourceList plugInSources);

        internal void SetApplicationOptions([NotNull] SilkyApplicationCreationOptions applicationOptions);

        internal void ConfigureRequestPipeline(IApplicationBuilder application);
        
        void RegisterDependencies(ContainerBuilder builder);

        void RegisterModules(ContainerBuilder containerBuilder);


        TOptions GetOptions<TOptions>()
            where TOptions : class, new();

        TOptions GetOptions<TOptions>(string optionName)
            where TOptions : class, new();
        

        T Resolve<T>(IServiceScope scope = null) where T : class;

        object Resolve(Type type, IServiceScope scope = null);
        object ResolveNamed(string name, Type type, IServiceScope scope = null);

        object ResolveServiceInstance(string? serviceKey, Type serviceType);

        T ResolveNamed<T>(string name,IServiceScope scope = null);

        IEnumerable<T> ResolveAll<T>();

        bool IsRegistered(Type type);

        bool IsRegisteredWithName(string name, Type type);

        object ResolveUnregistered(Type type);
    }
}