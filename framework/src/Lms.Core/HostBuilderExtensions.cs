using System.IO;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Lms.Core.Configuration;
using Lms.Core.Modularity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Lms.Core
{
    public static class HostBuilderExtensions
    {
        public static IHostBuilder RegisterLmsServices<T>(this IHostBuilder builder) where T : ILmsModule
        {
            AppSettings appSettings = null;
            IEngine engine = null;
            IServiceCollection services = null;
            builder.UseServiceProviderFactory(new AutofacServiceProviderFactory())
                .UseContentRoot(Directory.GetCurrentDirectory())
                .ConfigureServices((hostBuilder,config) =>
                {
                   (engine,appSettings) = config.ConfigureLmsServices(hostBuilder.Configuration, hostBuilder.HostingEnvironment);
                   services = config;
                })
                .ConfigureContainer<ContainerBuilder>(builder =>
                {
                    engine.RegisterDependencies(builder, appSettings);
                    engine.RegisterModules<T>(services,builder);
                })
                ;
            return builder;
        }
    }
}