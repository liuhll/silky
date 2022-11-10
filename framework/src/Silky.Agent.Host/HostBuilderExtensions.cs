using System;
using Microsoft.AspNetCore.Hosting;
using Silky.Core.Configuration;

namespace Microsoft.Extensions.Hosting
{
    public static class HostBuilderExtensions
    {
        public static IHostBuilder ConfigureSilkyWebHostDefaults(this IHostBuilder hostBuilder,
            Action<IWebHostBuilder> configure,
            Action<SilkyApplicationCreationOptions> optionsAction = null)
        {
            if (configure == null)
                throw new ArgumentNullException(nameof(configure));
            hostBuilder.RegisterSilkyServices<DefaultWebHostModule>(optionsAction);
            return hostBuilder.ConfigureWebHostDefaults(configure);
        }
        
        public static IHostBuilder ConfigureSilkyWebHostDefaults(this IHostBuilder hostBuilder,
            Action<IWebHostBuilder> configure,
            Action<WebHostBuilderOptions> configureAction,
            Action<SilkyApplicationCreationOptions> optionsAction = null)
        {
            if (configure == null)
                throw new ArgumentNullException(nameof(configure));
            if (configureAction == null)
                throw new ArgumentNullException(nameof(configureAction));
            hostBuilder.RegisterSilkyServices<DefaultWebHostModule>(optionsAction);
            return hostBuilder.ConfigureWebHost(configure, configureAction);
        }

        public static IHostBuilder ConfigureSilkyWebHost<T>(this IHostBuilder hostBuilder,
            Action<IWebHostBuilder> configure,
            Action<SilkyApplicationCreationOptions> optionsAction = null) where T : WebHostModule
        {
            if (configure == null)
                throw new ArgumentNullException(nameof(configure));
            hostBuilder.RegisterSilkyServices<T>(optionsAction);
            return hostBuilder.ConfigureWebHostDefaults(configure);
        }

        public static IHostBuilder ConfigureSilkyWebHost<T>(this IHostBuilder hostBuilder,
            Action<IWebHostBuilder> configure,
            Action<WebHostBuilderOptions> configureAction,
            Action<SilkyApplicationCreationOptions> optionsAction = null) where T : WebHostModule
        {
            if (configure == null)
                throw new ArgumentNullException(nameof(configure));
            if (configureAction == null)
                throw new ArgumentNullException(nameof(configureAction));
            hostBuilder.RegisterSilkyServices<T>(optionsAction);
            return hostBuilder.ConfigureWebHost(configure, configureAction);
        }

        public static IHostBuilder ConfigureSilkyGatewayDefaults(this IHostBuilder hostBuilder,
            Action<IWebHostBuilder> configure,
            Action<SilkyApplicationCreationOptions> optionsAction = null)
        {
            if (configure == null)
                throw new ArgumentNullException(nameof(configure));
            hostBuilder.RegisterSilkyServices<DefaultGatewayHostModule>(optionsAction);
            return hostBuilder.ConfigureWebHostDefaults(configure);
        }

        public static IHostBuilder ConfigureSilkyGatewayDefaults(this IHostBuilder hostBuilder,
            Action<IWebHostBuilder> configure,
            Action<WebHostBuilderOptions> configureAction,
            Action<SilkyApplicationCreationOptions> optionsAction = null)
        {
            if (configure == null)
                throw new ArgumentNullException(nameof(configure));
            if (configureAction == null)
                throw new ArgumentNullException(nameof(configureAction));
            hostBuilder.RegisterSilkyServices<DefaultGatewayHostModule>(optionsAction);
            return hostBuilder.ConfigureWebHost(configure, configureAction);
        }

        public static IHostBuilder ConfigureSilkyGateway<T>(this IHostBuilder hostBuilder,
            Action<IWebHostBuilder> configure,
            Action<SilkyApplicationCreationOptions> optionsAction = null)
            where T : GatewayHostModule
        {
            if (configure == null)
                throw new ArgumentNullException(nameof(configure));
            hostBuilder.RegisterSilkyServices<T>(optionsAction);
            return hostBuilder.ConfigureWebHostDefaults(configure);
        }

        public static IHostBuilder ConfigureSilkyGateway<T>(this IHostBuilder hostBuilder,
            Action<IWebHostBuilder> configure,
            Action<WebHostBuilderOptions> configureAction,
            Action<SilkyApplicationCreationOptions> optionsAction = null)
            where T : GatewayHostModule
        {
            if (configure == null)
                throw new ArgumentNullException(nameof(configure));
            if (configureAction == null)
                throw new ArgumentNullException(nameof(configureAction));
            hostBuilder.RegisterSilkyServices<T>(optionsAction);
            return hostBuilder.ConfigureWebHost(configure, configureAction);
        }

        public static IHostBuilder ConfigureSilkyGeneralHostDefaults(this IHostBuilder hostBuilder,
            Action<SilkyApplicationCreationOptions> optionsAction = null)
        {
            hostBuilder.RegisterSilkyServices<DefaultGeneralHostModule>(optionsAction);
            return hostBuilder;
        }

        public static IHostBuilder ConfigureSilkyGeneralHost<T>(this IHostBuilder hostBuilder,
            Action<SilkyApplicationCreationOptions> optionsAction = null)
            where T : GeneralHostModule
        {
            hostBuilder.RegisterSilkyServices<T>(optionsAction);
            return hostBuilder;
        }

        public static IHostBuilder ConfigureSilkyWebSocketDefaults(this IHostBuilder hostBuilder,
            Action<SilkyApplicationCreationOptions> optionsAction = null)
        {
            hostBuilder.RegisterSilkyServices<DefaultWebSocketHostModule>(optionsAction);
            return hostBuilder;
        }

        public static IHostBuilder ConfigureSilkyWebSocket<T>(this IHostBuilder hostBuilder,
            Action<SilkyApplicationCreationOptions> optionsAction = null)
            where T : WebSocketHostModule
        {
            hostBuilder.RegisterSilkyServices<T>(optionsAction);
            return hostBuilder;
        }
    }
}