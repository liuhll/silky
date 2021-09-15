namespace Microsoft.Extensions.Hosting
{
    public static class HostBuilderExtensions
    {
        public static IHostBuilder ConfigureSilkyWebHostDefaults(this IHostBuilder hostBuilder)
        {
            hostBuilder.RegisterSilkyServices<WebHostModule>();
            return hostBuilder;
        }
    }
}