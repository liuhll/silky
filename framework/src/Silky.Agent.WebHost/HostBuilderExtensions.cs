namespace Microsoft.Extensions.Hosting
{
    public static class HostBuilderExtensions
    {
        public static IHostBuilder ConfigureSilkyWebHostDefaults(this IHostBuilder hostBuilder)
        {
            hostBuilder.RegisterSilkyServices<DefaultWebHostModule>();
            return hostBuilder;
        }
        
        public static IHostBuilder ConfigureSilkyWebHost<T>(this IHostBuilder hostBuilder) where T : WebHostModule
        {
            hostBuilder.RegisterSilkyServices<T>();
            return hostBuilder;
        }
    }
}