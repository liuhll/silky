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
        public static IEngine ConfigureSilkyServices(this IServiceCollection services, IConfiguration configuration,
            IHostEnvironment hostEnvironment)
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            CommonHelper.DefaultFileProvider = new SilkyFileProvider(hostEnvironment);
            var engine = EngineContext.Create();
            var moduleLoder = new ModuleLoader();
            services.TryAddSingleton<IModuleLoader>(moduleLoder);
            services.AddOptions<AppSettingsOptions>()
                .Bind(configuration.GetSection(AppSettingsOptions.AppSettings));
            engine.ConfigureServices(services, configuration, hostEnvironment);
            return engine;
        }
    }
}