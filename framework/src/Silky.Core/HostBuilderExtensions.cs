using System;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Hosting;
using Silky.Core;
using Silky.Core.Modularity;
using Microsoft.Extensions.DependencyInjection;
using Silky.Core.Configuration;
using Silky.Core.Extensions;

namespace Microsoft.Extensions.Hosting
{
    public static class HostBuilderExtensions
    {
        public static IHostBuilder RegisterSilkyServices<T>(
            this IHostBuilder builder,
            Action<SilkyApplicationCreationOptions> optionsAction = null)
            where T : SilkyModule
        {
            IEngine engine = null;
            IServiceCollection services = null;
            SilkyApplicationCreationOptions options = null;

            builder
                .UseServiceProviderFactory(new AutofacServiceProviderFactory())
                .ConfigureServices((hostBuilder, sc) =>
                {
                    options = new SilkyApplicationCreationOptions(sc);
                    optionsAction?.Invoke(options);
                    var configuration = ConfigurationHelper.BuildConfiguration(
                        hostBuilder.HostingEnvironment,
                        options.Configuration);
                    hostBuilder.Configuration = configuration;
                    sc.ReplaceConfiguration(configuration);
                    engine = sc.AddSilkyServices<T>(hostBuilder.Configuration,
                        hostBuilder.HostingEnvironment, options);
                    services = sc;
                })
                .ConfigureContainer<ContainerBuilder>(builder =>
                {
                    engine.RegisterModules(services, builder);
                    engine.RegisterDependencies(builder);
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