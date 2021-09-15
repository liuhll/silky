namespace Microsoft.Extensions.Hosting
{
    public static class HostBuilderExtensions
    {
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