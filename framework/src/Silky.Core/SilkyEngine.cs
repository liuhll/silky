using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Silky.Core.Configuration;
using Silky.Core.Extensions;
using Silky.Core.DependencyInjection;
using Silky.Core.Exceptions;
using Silky.Core.Modularity;
using Silky.Core.Modularity.PlugIns;
using Silky.Core.Reflection;
using Silky.Core.Runtime.Rpc;

namespace Silky.Core
{
    internal sealed class SilkyEngine : IEngine, IModuleContainer
    {
        private ITypeFinder _typeFinder;
        private Banner _banner;


        public IConfiguration Configuration { get; private set; }

        public IHostEnvironment HostEnvironment { get; private set; }

        Banner IEngine.Banner
        {
            get => _banner;
            set => _banner = value;
        }

        public string HostName { get; private set; }

        internal SilkyEngine()
        {
            HostName = Assembly.GetEntryAssembly()?.GetName().Name;
            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
        }

        void IEngine.SetTypeFinder([NotNull] IServiceCollection services, [NotNull] ISilkyFileProvider fileProvider,
            [NotNull] AppServicePlugInSourceList appServicePlugInSources)
        {
            Check.NotNull(services, nameof(services));
            Check.NotNull(fileProvider, nameof(fileProvider));
            Check.NotNull(appServicePlugInSources, nameof(appServicePlugInSources));

            _typeFinder = new SilkyAppTypeFinder(appServicePlugInSources, fileProvider);
            services.AddSingleton(_typeFinder);
        }

        void IEngine.SetHostEnvironment([NotNull] IHostEnvironment hostEnvironment)
        {
            Check.NotNull(hostEnvironment, nameof(hostEnvironment));
            HostEnvironment = hostEnvironment;
        }


        void IEngine.SetConfiguration([NotNull] IConfiguration configuration)
        {
            Check.NotNull(configuration, nameof(configuration));
            Configuration = configuration;
        }

        void IEngine.ConfigureServices([NotNull] IServiceCollection services, [NotNull] IConfiguration configuration)
        {
            Check.NotNull(services, nameof(services));
            Check.NotNull(configuration, nameof(configuration));
            var configureServices = _typeFinder.FindClassesOfType<IConfigureService>();
            //create and sort instances of startup configurations
            var instances = configureServices
                .Select(configureService => (IConfigureService)Activator.CreateInstance(configureService));

            //configure services
            foreach (var instance in instances)
                instance.ConfigureServices(services, Configuration);
            // configure modules 
            foreach (var module in Modules)
                module.Instance.ConfigureServices(services, Configuration);
            ServiceProvider = services.BuildServiceProvider();
        }

        public TOptions GetOptions<TOptions>()
            where TOptions : class, new()
        {
            return Resolve<IOptions<TOptions>>()?.Value;
        }

        public TOptions GetOptions<TOptions>(string optionName)
            where TOptions : class, new()
        {
            var options = GetOptions<TOptions>();
            if (options != null)
            {
                return options;
            }

            options = Configuration.GetSection(optionName).Get<TOptions>() ?? new TOptions();
            return options;
        }

        public TOptions GetOptionsMonitor<TOptions>()
            where TOptions : class, new()
        {
            return Resolve<IOptionsMonitor<TOptions>>()?.CurrentValue;
        }

        public TOptions GetOptionsMonitor<TOptions>(Action<TOptions, string> listener)
            where TOptions : class, new()
        {
            var optionsMonitor = Resolve<IOptionsMonitor<TOptions>>();
            if (optionsMonitor != null)
            {
                optionsMonitor.OnChange(listener);
            }

            return optionsMonitor?.CurrentValue;
        }


        public TOptions GetOptionsSnapshot<TOptions>()
            where TOptions : class, new()
        {
            return Resolve<IOptionsSnapshot<TOptions>>()?.Value;
        }

        public ITypeFinder TypeFinder => _typeFinder;


        void IEngine.ConfigureRequestPipeline(IApplicationBuilder application)
        {
            ServiceProvider = application.ApplicationServices;
            var typeFinder = Resolve<ITypeFinder>();
            var startupConfigurations = typeFinder.FindClassesOfType<ISilkyStartup>();

            //create and sort instances of startup configurations
            var instances = startupConfigurations
                .Select(startup => (ISilkyStartup)Activator.CreateInstance(startup))
                .OrderBy(startup => startup.Order);

            //configure request pipeline
            foreach (var instance in instances)
                instance.Configure(application);

            var webSilkyModules = Modules.Where(p => p.Instance is HttpSilkyModule);
            foreach (var webSilkyModule in webSilkyModules)
            {
                ((HttpSilkyModule)webSilkyModule.Instance).Configure(application);
            }
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

        public object ResolveNamed(string name, Type type)
        {
            var sp = GetServiceProvider();
            if (sp == null)
                return null;
            return sp.GetAutofacRoot().ResolveNamed(name, type);
        }

        public object ResolveServiceInstance(string serviceKey, Type serviceType)
        {
            object instance = null;
            if (!serviceKey.IsNullOrEmpty())
            {
                if (!EngineContext.Current.IsRegisteredWithName(serviceKey, serviceType))
                {
                    throw new UnServiceKeyImplementationException(
                        $"There is no implementation class of the {serviceType.FullName} interface whose serviceKey is {serviceKey} in the system");
                }

                instance = EngineContext.Current.ResolveNamed(serviceKey, serviceType);
            }
            else
            {
                instance = EngineContext.Current.Resolve(serviceType);
            }

            return instance;
        }

        public T ResolveNamed<T>(string name)
        {
            return (T)ResolveNamed(name, typeof(T));
        }

        public IEnumerable<T> ResolveAll<T>()
        {
            return (IEnumerable<T>)GetServiceProvider().GetServices(typeof(T));
        }

        public bool IsRegistered(Type type)
        {
            return GetServiceProvider().GetAutofacRoot().IsRegistered(type);
        }

        public bool IsRegisteredWithName(string name, Type type)
        {
            return GetServiceProvider().GetAutofacRoot().IsRegisteredWithName(name, type);
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
                            throw new SilkyException("Unknown dependency");
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

            throw new SilkyException("No constructor was found that had all the dependencies satisfied.",
                innerException);
        }

        private IServiceProvider GetServiceProvider()
        {
            if (ServiceProvider == null)
                return null;
            var serviceProvider = ServiceProvider;
            var httpContextAccessor = serviceProvider?.GetService<IHttpContextAccessor>();
            if (httpContextAccessor is { HttpContext: { } })
            {
                serviceProvider = httpContextAccessor.HttpContext.RequestServices;
            }

            var rpcContextAccessor = serviceProvider?.GetService<IRpcContextAccessor>();
            if (rpcContextAccessor is { RpcContext: { RpcServices: { } } })
            {
                serviceProvider = rpcContextAccessor.RpcContext.RpcServices;
            }

            return serviceProvider;
        }

        void IEngine.SetApplicationName([NotNull] string applicationName)
        {
            Check.NotNullOrWhiteSpace(applicationName, nameof(applicationName));
            HostName = applicationName;
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
            var tf = _typeFinder;
            if (tf == null)
                return null;
            assembly = tf.GetAssemblies().FirstOrDefault(a => a.FullName == args.Name);
            return assembly;
        }

        public void RegisterModules(IServiceCollection services, ContainerBuilder containerBuilder)
        {
            containerBuilder.RegisterInstance(this).As<IModuleContainer>().SingleInstance();
            var assemblyNames = ((AppDomainTypeFinder)_typeFinder).AssemblyNames;
            foreach (var module in Modules)
            {
                if (!assemblyNames.Contains(module.Assembly.FullName))
                {
                    ((AppDomainTypeFinder)_typeFinder).AssemblyNames.Add(module.Assembly.FullName);
                }

                containerBuilder.RegisterModule((SilkyModule)module.Instance);
            }
        }

        void IEngine.LoadModules(IServiceCollection services, Type startUpType, IModuleLoader moduleLoader,
            [NotNull] PlugInSourceList plugInSources)
        {
            Check.NotNull(plugInSources, nameof(plugInSources));
            if (!SilkyModule.IsSilkyModule(startUpType))
            {
                throw new SilkyException($"{startUpType.FullName} is not a Silky module type.");
            }

            LoadConfigPlugInModules(plugInSources);

            Modules = moduleLoader.LoadModules(services, startUpType, plugInSources);
        }

        private void LoadConfigPlugInModules(PlugInSourceList plugInSources)
        {
            var plugInOptions = GetOptions<PlugInSourceOptions>(PlugInSourceOptions.PlugInSource);

            if (plugInOptions.ModulePlugIn == null) return;
            if (plugInOptions.ModulePlugIn.Types != null)
            {
                plugInSources.AddTypeNames(plugInOptions.ModulePlugIn.Types);
            }

            if (plugInOptions.ModulePlugIn.FilePaths != null)
            {
                plugInSources.AddFiles(plugInOptions.ModulePlugIn.FilePaths);
            }

            if (plugInOptions.ModulePlugIn.Folders != null)
            {
                plugInSources.AddFolders(plugInOptions.ModulePlugIn.Folders);
            }
        }

        public IServiceProvider ServiceProvider { get; set; }

        public IReadOnlyList<ISilkyModuleDescriptor> Modules { get; private set; }
    }
}