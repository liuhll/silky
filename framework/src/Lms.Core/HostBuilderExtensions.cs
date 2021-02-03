using System.IO;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Lms.Core.Modularity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

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
                   
                })
                .ConfigureContainer<ContainerBuilder>(builder =>
                {
                    engine.RegisterModules<T>(services,builder);
                    engine.RegisterDependencies(builder);
                }).ConfigureLogging(logging =>
                {
                   
                })
                ;
            return builder;
        }
        
    }
}