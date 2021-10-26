using System.IO;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Silky.Core;
using Silky.Core.Modularity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Silky.Core.Extensions;

namespace Microsoft.Extensions.Hosting
{
    public static class HostBuilderExtensions
    {
        public static IHostBuilder RegisterSilkyServices<T>(this IHostBuilder builder) where T : StartUpModule
        {
            IEngine engine = null;
            IServiceCollection services = null;
            builder.UseServiceProviderFactory(new AutofacServiceProviderFactory())
                .UseContentRoot(Directory.GetCurrentDirectory())
                .ConfigureServices((hostBuilder, sc) =>
                {
                    engine = sc.AddSilkyServices<T>(hostBuilder.Configuration,
                        hostBuilder.HostingEnvironment);
                    services = sc;
                })
                .ConfigureContainer<ContainerBuilder>(builder =>
                {
                    engine.RegisterModules(services, builder);
                    engine.RegisterDependencies(builder);
                })
                .ConfigureAppConfiguration((hosting, config) =>
                {
                    config.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                        .AddJsonFile($"appsettings.{hosting.HostingEnvironment.EnvironmentName}.json", optional: true,
                            true);

                    // Adds YAML settings later
                    config.AddYamlFile("appsettings.yml", optional: true, true)
                        .AddYamlFile($"appsettings.{hosting.HostingEnvironment.EnvironmentName}.yml", optional: true,
                            true)
                        .AddYamlFile("appsettings.yaml", optional: true, true)
                        .AddYamlFile($"appsettings.{hosting.HostingEnvironment.EnvironmentName}.yaml", optional: true,
                            true);

                    // add RateLimit configfile
                    config.AddJsonFile("ratelimit.json", optional: true, reloadOnChange: true)
                        .AddJsonFile($"ratelimit.{hosting.HostingEnvironment.EnvironmentName}.json", optional: true,
                            true);
                    config.AddYamlFile("ratelimit.yml", optional: true, reloadOnChange: true)
                        .AddYamlFile($"ratelimit.{hosting.HostingEnvironment.EnvironmentName}.yml", optional: true,
                            true)
                        .AddYamlFile("ratelimit.yaml", optional: true, reloadOnChange: true)
                        .AddYamlFile($"ratelimit.{hosting.HostingEnvironment.EnvironmentName}.yaml", optional: true,
                            true);
                    config.AddEnvironmentVariables();
                })
                ;

            return builder;
        }

        public static bool IsEnvironment(this IHostBuilder builder, string environmentName)
        {
            return EngineContext.Current.IsEnvironment(environmentName);
        }
    }
}