using System;
using Microsoft.AspNetCore.Hosting;

namespace Microsoft.Extensions.Hosting
{
    public static class HostBuilderExtensions
    {
        public static IHostBuilder ConfigureSilkyWebHostDefaults(this IHostBuilder hostBuilder,
            Action<IWebHostBuilder> configure)
        {
            if (configure == null)
                throw new ArgumentNullException(nameof(configure));
            hostBuilder.RegisterSilkyServices<DefaultWebHostModule>();
            return hostBuilder.ConfigureWebHostDefaults(configure);
        }

        public static IHostBuilder ConfigureSilkyWebHost<T>(this IHostBuilder hostBuilder,
            Action<IWebHostBuilder> configure) where T : WebHostModule
        {
            if (configure == null)
                throw new ArgumentNullException(nameof(configure));
            hostBuilder.RegisterSilkyServices<T>();
            return hostBuilder.ConfigureWebHostDefaults(configure);
        }

        public static IHostBuilder ConfigureSilkyGatewayDefaults(this IHostBuilder hostBuilder,
            Action<IWebHostBuilder> configure)
        {
            if (configure == null)
                throw new ArgumentNullException(nameof(configure));
            hostBuilder.RegisterSilkyServices<DefaultGatewayHostModule>();
            return hostBuilder.ConfigureWebHostDefaults(configure);
        }

        public static IHostBuilder ConfigureSilkyGateway<T>(this IHostBuilder hostBuilder,
            Action<IWebHostBuilder> configure) where T : GatewayHostModule
        {
            if (configure == null)
                throw new ArgumentNullException(nameof(configure));
            hostBuilder.RegisterSilkyServices<T>();
            return hostBuilder.ConfigureWebHostDefaults(configure);
        }

        public static IHostBuilder ConfigureSilkyGeneralHostDefaults(this IHostBuilder hostBuilder)
        {
            hostBuilder.RegisterSilkyServices<DefaultGeneralHostModule>();
            return hostBuilder;
        }

        public static IHostBuilder ConfigureSilkyGeneralHost<T>(this IHostBuilder hostBuilder)
            where T : GeneralHostModule
        {
            hostBuilder.RegisterSilkyServices<T>();
            return hostBuilder;
        }

        public static IHostBuilder ConfigureSilkyWebSocketDefaults(this IHostBuilder hostBuilder)
        {
            hostBuilder.RegisterSilkyServices<DefaultWebSocketHostModule>();
            return hostBuilder;
        }

        public static IHostBuilder ConfigureSilkyWebSocket<T>(this IHostBuilder hostBuilder)
            where T : WebSocketHostModule
        {
            hostBuilder.RegisterSilkyServices<T>();
            return hostBuilder;
        }
    }
}