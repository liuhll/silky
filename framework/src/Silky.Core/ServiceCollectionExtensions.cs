using System.Net;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Silky.Core.Configuration;
using Silky.Core.Modularity;
using Silky.Core.Threading;

namespace Silky.Core
{
    public static class ServiceCollectionExtensions
    {
        public static IEngine AddSilkyServices<T>(this IServiceCollection services, IConfiguration configuration,
            IHostEnvironment hostEnvironment) where T : StartUpModule
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            CommonSilkyHelpers.DefaultFileProvider = new SilkyFileProvider(hostEnvironment);
            services.TryAddSingleton(CommonSilkyHelpers.DefaultFileProvider);
            var engine = EngineContext.Create();
            services.AddOptions<AppSettingsOptions>()
                .Bind(configuration.GetSection(AppSettingsOptions.AppSettings));
            var moduleLoader = new ModuleLoader();
            engine.LoadModules<T>(services, moduleLoader);
            services.TryAddSingleton<IModuleLoader>(moduleLoader);
            services.AddHostedService<InitSilkyHostedService>();
            services.AddSingleton<ICancellationTokenProvider>(NullCancellationTokenProvider.Instance);
            engine.ConfigureServices(services, configuration, hostEnvironment);
            return engine;
        }
    }
}