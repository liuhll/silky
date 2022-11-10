using System;
using Autofac;
using Autofac.Extensions.DependencyInjection;
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
            SilkyApplicationCreationOptions applicationCreationOptions = null;

            builder
                .UseServiceProviderFactory(new AutofacServiceProviderFactory())
                .ConfigureServices((hostBuilder, sc) =>
                {
                    applicationCreationOptions = new SilkyApplicationCreationOptions(sc);
                    optionsAction?.Invoke(applicationCreationOptions);
                    var configuration = ConfigurationHelper.BuildConfiguration(
                        hostBuilder.HostingEnvironment,
                        applicationCreationOptions.Configuration);
                    hostBuilder.Configuration = configuration;
                    sc.ReplaceConfiguration(configuration);
                    engine = sc.AddSilkyServices<T>(hostBuilder.Configuration,
                        hostBuilder.HostingEnvironment, applicationCreationOptions);
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