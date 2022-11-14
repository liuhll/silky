using System;
using System.Net;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Silky.Core.Configuration;
using Silky.Core.Extensions;
using Silky.Core.Logging;
using Silky.Core.Modularity;
using Silky.Core.Threading;

namespace Silky.Core
{
    public static class ServiceCollectionExtensions
    {
        public static IEngine AddSilkyServices<T>(this IServiceCollection services, IConfiguration configuration,
            IHostEnvironment hostEnvironment, SilkyApplicationCreationOptions options) where T : SilkyModule
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            CommonSilkyHelpers.DefaultFileProvider = new SilkyFileProvider(hostEnvironment);
            services.TryAddSingleton(CommonSilkyHelpers.DefaultFileProvider);
            services.TryAddSingleton<IInitLoggerFactory>(new DefaultInitLoggerFactory());
            services.TryAddTransient<IBannerPrinter, BannerPrinter>();
            services.AddHostedService<InitSilkyHostedService>();
            services.AddSingleton<ICancellationTokenProvider>(NullCancellationTokenProvider.Instance);
            var moduleLoader = new ModuleLoader();
            services.TryAddSingleton<IModuleLoader>(moduleLoader);

            services.AddOptions<AppSettingsOptions>()
                .Bind(configuration.GetSection(AppSettingsOptions.AppSettings));
            services.AddOptions<PlugInSourceOptions>()
                .Bind(configuration.GetSection(PlugInSourceOptions.PlugInSource));

            var engine = EngineContext.Create();
            if (!options.ApplicationName.IsNullOrWhiteSpace())
            {
                engine.SetApplicationName(options.ApplicationName);
            }
            
            engine.Banner = BannerHelper.BuildBanner(options);
            engine.SetHostEnvironment(hostEnvironment);
            engine.SetConfiguration(configuration);
            engine.SetTypeFinder(services, CommonSilkyHelpers.DefaultFileProvider, options.AppServicePlugInSources);
            engine.LoadModules(services, typeof(T), moduleLoader, options.ModulePlugInSources);
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