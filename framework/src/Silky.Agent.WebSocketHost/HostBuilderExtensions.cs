using Silky.Core.Modularity;

namespace Microsoft.Extensions.Hosting
{
    public static class HostBuilderExtensions
    {
        public static IHostBuilder ConfigureSilkyWebSocketDefaults(this IHostBuilder hostBuilder)
        {
            hostBuilder.RegisterSilkyServices<WebSocketHostModule>();
            return hostBuilder;
        }
        
        public static IHostBuilder ConfigureSilkyWebSocket<T>(this IHostBuilder hostBuilder) where T : StartUpModule
        {
            hostBuilder.RegisterSilkyServices<T>();
            return hostBuilder;
        }
    }
}