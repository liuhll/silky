using System.IO;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Lms.Core;
using Lms.Core.Modularity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.Extensions.Hosting
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