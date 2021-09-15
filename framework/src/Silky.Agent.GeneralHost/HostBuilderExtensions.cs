namespace Microsoft.Extensions.Hosting
{
    public static class HostBuilderExtensions
    {
        public static IHostBuilder ConfigureSilkyGeneralHostDefaults(this IHostBuilder hostBuilder)
        {
            hostBuilder.RegisterSilkyServices<GeneralHostModule>();
            return hostBuilder;
        }
    }
}