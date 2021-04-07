using System.Net;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Silky.Lms.Core.Modularity;

namespace Silky.Lms.Core
{
    public static class ServiceCollectionExtensions
    {
        public static IEngine ConfigureLmsServices(this IServiceCollection services, IConfiguration configuration, IHostEnvironment hostEnvironment)
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            CommonHelper.DefaultFileProvider = new LmsFileProvider(hostEnvironment);
            var engine = EngineContext.Create();
            var moduleLoder = new ModuleLoader();
            services.TryAddSingleton<IModuleLoader>(moduleLoder);
            engine.ConfigureServices(services, configuration);
            return engine;
        }
    }
}