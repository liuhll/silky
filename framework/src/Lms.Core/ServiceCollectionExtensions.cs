using System.Net;
using Lms.Core.Modularity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;

namespace Lms.Core
{
    public static class ServiceCollectionExtensions
    {
        public static IEngine ConfigureLmsServices(this IServiceCollection services, IConfiguration configuration, IHostEnvironment hostEnvironment)
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            services.AddHttpContextAccessor();
            CommonHelper.DefaultFileProvider = new LmsFileProvider(hostEnvironment);
            var engine = EngineContext.Create();
            var moduleLoder = new ModuleLoader();
            services.TryAddSingleton<IModuleLoader>(moduleLoder);
            engine.ConfigureServices(services, configuration);
            return engine;
        }
    }
}