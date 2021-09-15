namespace Microsoft.Extensions.Hosting
{
    public static class HostBuilderExtensions
    {
        public static IHostBuilder ConfigureSilkyGeneralHostDefaults(this IHostBuilder hostBuilder)
        {
            hostBuilder.RegisterSilkyServices<DefaultGeneralHostModule>();
            return hostBuilder;
        }
        
        public static IHostBuilder ConfigureSilkyGeneralHost<T>(this IHostBuilder hostBuilder) where T : GeneralHostModule
        {
            hostBuilder.RegisterSilkyServices<T>();
            return hostBuilder;
        }
    }
}