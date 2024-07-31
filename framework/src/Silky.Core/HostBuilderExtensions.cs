using System;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Silky.Core;
using Silky.Core.Modularity;
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
            var options = new SilkyApplicationCreationOptions();
            optionsAction?.Invoke(options);

            builder
                .UseServiceProviderFactory(new AutofacServiceProviderFactory())
                .ConfigureServices((context, services) =>
                {
                    engine = services.AddSilkyServices<T>(
                        context.Configuration,
                        context.HostingEnvironment,
                        options
                    );

                    var bannerPrinter = engine.Resolve<IBannerPrinter>();
                    bannerPrinter.Print();
                })
                .ConfigureContainer<ContainerBuilder>(containerBuilder =>
                {
                    engine!.RegisterModules(containerBuilder);
                    engine!.RegisterDependencies(containerBuilder);
                }).ConfigureAppConfiguration((hostBuilder, configurationBuilder) =>
                {
                    hostBuilder.Configuration = ConfigurationHelper
                        .BuildConfiguration(configurationBuilder,
                            hostBuilder.HostingEnvironment,
                            options.Configuration);
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