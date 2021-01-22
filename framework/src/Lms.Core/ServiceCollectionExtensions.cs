using System.Net;
using Lms.Core.Configuration;
using Lms.Core.Modularity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;

namespace Lms.Core
{
    public static class ServiceCollectionExtensions
    {
        public static (IEngine, AppSettings) ConfigureLmsServices(this IServiceCollection services, IConfiguration configuration, IHostEnvironment hostEnvironment)
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            services.AddHttpContextAccessor();
            CommonHelper.DefaultFileProvider = new LmsFileProvider(hostEnvironment);
            var engine = EnginContext.Create();
            engine.ConfigureServices(services, configuration);
            
            var appSettings = new AppSettings();
            configuration.Bind(appSettings);
            services.AddSingleton(appSettings);
            var moduleLoder = new ModuleLoader();
            services.TryAddSingleton<IModuleLoader>(moduleLoder);
            return (engine, appSettings);
        }
    }
}