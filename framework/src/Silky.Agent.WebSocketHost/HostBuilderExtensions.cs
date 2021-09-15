namespace Microsoft.Extensions.Hosting
{
    public static class HostBuilderExtensions
    {
        public static IHostBuilder ConfigureSilkyWebSocketDefaults(this IHostBuilder hostBuilder)
        {
            hostBuilder.RegisterSilkyServices<WebSocketHostModule>();
            return hostBuilder;
        }
    }
}