using System;
using System.Collections.Generic;
using Autofac;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Silky.Core.Modularity;
using Silky.Core.Modularity.PlugIns;
using Silky.Core.Reflection;

namespace Silky.Core
{
    public interface IEngine
    {
        IServiceProvider ServiceProvider { get; set; }

        ITypeFinder TypeFinder { get; }

        IConfiguration Configuration { get; }

        IHostEnvironment HostEnvironment { get; }

        string HostName { get; }

        internal void SetHostEnvironment([NotNull] IHostEnvironment hostEnvironment);

        internal void SetConfiguration([NotNull] IConfiguration configuration);

        internal void ConfigureServices(IServiceCollection services, IConfiguration configuration);

        public void RegisterDependencies(ContainerBuilder builder);

        public void RegisterModules(IServiceCollection services, ContainerBuilder containerBuilder);

        internal void LoadModules(IServiceCollection services, Type startUpType, IModuleLoader moduleLoader,
            [NotNull] PlugInSourceList plugInSources);

        TOptions GetOptions<TOptions>()
            where TOptions : class, new();

        TOptions GetOptions<TOptions>(string optionName)
            where TOptions : class, new();

        TOptions GetOptionsMonitor<TOptions>()
            where TOptions : class, new();

        TOptions GetOptionsMonitor<TOptions>(Action<TOptions, string> listener)
            where TOptions : class, new();

        TOptions GetOptionsSnapshot<TOptions>()
            where TOptions : class, new();

        void ConfigureRequestPipeline(IApplicationBuilder application);

        T Resolve<T>() where T : class;

        object Resolve(Type type);
        object ResolveNamed(string name, Type type);

        object ResolveServiceInstance(string serviceKey, Type serviceType);

        T ResolveNamed<T>(string name);

        IEnumerable<T> ResolveAll<T>();

        bool IsRegistered(Type type);

        bool IsRegisteredWithName(string name, Type type);

        object ResolveUnregistered(Type type);
    }
}