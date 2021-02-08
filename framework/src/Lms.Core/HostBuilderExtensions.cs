using System.IO;
using System.Linq;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Lms.Core.Modularity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Lms.Core
{
    public static class HostBuilderExtensions
    {
        public static IHostBuilder RegisterLmsServices<T>(this IHostBuilder builder) where T : ILmsModule
        {
            IEngine engine = null;
            IServiceCollection services = null;
            builder.UseServiceProviderFactory(new AutofacServiceProviderFactory())
                .UseContentRoot(Directory.GetCurrentDirectory())
                .ConfigureServices((hostBuilder,config) =>
                {
                   (engine) = config.ConfigureLmsServices(hostBuilder.Configuration, hostBuilder.HostingEnvironment);
                   services = config;
                   services.AddHostedService<InitLmsHostedService>();

                })
                .ConfigureContainer<ContainerBuilder>(builder =>
                {
                    engine.RegisterModules<T>(services,builder);
                    engine.RegisterDependencies(builder);
                })
                .ConfigureAppConfiguration((hosting,config) =>
                {
                    config.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                        .AddJsonFile($"appsettings.{hosting.HostingEnvironment.EnvironmentName}.json", optional: true);

                    // Adds YAML settings later
                    config.AddYamlFile("appsettings.yml", optional: true) 
                        .AddYamlFile($"appsettings.{hosting.HostingEnvironment.EnvironmentName}.yml", optional: true);
                        
                })
                // .ConfigureLogging(logging =>
                // {
                //     
                // })
                ;
            return builder;
        }
        
    }
}