using System.IO;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Lms.Core.Configuration;
using Microsoft.Extensions.Hosting;

namespace Lms.Core
{
    public static class HostBuilderExtensions
    {
        public static IHostBuilder RegisterLmsServices(this IHostBuilder builder)
        {
            AppSettings appSettings = null;
            IEngine engine = null;
            builder.UseServiceProviderFactory(new AutofacServiceProviderFactory())
                .UseContentRoot(Directory.GetCurrentDirectory())
                .ConfigureServices((hostBuilder,config) =>
                {
                   (engine,appSettings) = config.ConfigureLmsServices(hostBuilder.Configuration, hostBuilder.HostingEnvironment);
                })
                .ConfigureContainer<ContainerBuilder>(builder =>
                {
                    engine.RegisterDependencies(builder, appSettings);
                })
                ;
            return builder;
        }
    }
}