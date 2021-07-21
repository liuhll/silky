using System.Net;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Silky.Core.Configuration;
using Silky.Core.Modularity;

namespace Silky.Core
{
    public static class ServiceCollectionExtensions
    {
        public static IEngine ConfigureSilkyServices<T>(this IServiceCollection services, IConfiguration configuration,
            IHostEnvironment hostEnvironment) where T : StartUpModule
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            CommonHelper.DefaultFileProvider = new SilkyFileProvider(hostEnvironment);
            var engine = EngineContext.Create();
            var moduleLoader = new ModuleLoader();
            engine.LoadModules<T>(services, moduleLoader);
            services.TryAddSingleton<IModuleLoader>(moduleLoader);
            services.AddOptions<AppSettingsOptions>()
                .Bind(configuration.GetSection(AppSettingsOptions.AppSettings));
            engine.ConfigureServices(services, configuration, hostEnvironment);
            return engine;
        }
    }
}