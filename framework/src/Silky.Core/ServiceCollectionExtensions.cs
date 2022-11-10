using System;
using System.Net;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Silky.Core.Configuration;
using Silky.Core.Logging;
using Silky.Core.Modularity;
using Silky.Core.Threading;

namespace Silky.Core
{
    public static class ServiceCollectionExtensions
    {
        public static IEngine AddSilkyServices<T>(this IServiceCollection services, IConfiguration configuration,
            IHostEnvironment hostEnvironment, SilkyApplicationCreationOptions options) where T : StartUpModule
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            CommonSilkyHelpers.DefaultFileProvider = new SilkyFileProvider(hostEnvironment);
            services.TryAddSingleton(CommonSilkyHelpers.DefaultFileProvider);
            services.TryAddSingleton<IInitLoggerFactory>(new DefaultInitLoggerFactory());
            services.AddHostedService<InitSilkyHostedService>();
            services.AddSingleton<ICancellationTokenProvider>(NullCancellationTokenProvider.Instance);
            services.AddOptions<AppSettingsOptions>()
                .Bind(configuration.GetSection(AppSettingsOptions.AppSettings));
            services.AddOptions<PlugInSourceOptions>()
                .Bind(configuration.GetSection(PlugInSourceOptions.PlugInSource));
            var engine = EngineContext.Create();
            engine.SetHostEnvironment(hostEnvironment);
            engine.SetConfiguration(configuration);

            var moduleLoader = new ModuleLoader();
            engine.LoadModules<T>(services, moduleLoader, options.PlugInSources);
            services.TryAddSingleton<IModuleLoader>(moduleLoader);
            engine.ConfigureServices(services, configuration);
            return engine;
        }

        public static IServiceCollection ReplaceConfiguration(this IServiceCollection services,
            IConfiguration configuration)
        {
            return services.Replace(ServiceDescriptor.Singleton<IConfiguration>(configuration));
        }
    }
}