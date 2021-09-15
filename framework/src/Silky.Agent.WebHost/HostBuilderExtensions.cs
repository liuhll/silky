using Silky.Core.Modularity;

namespace Microsoft.Extensions.Hosting
{
    public static class HostBuilderExtensions
    {
        public static IHostBuilder ConfigureSilkyWebHostDefaults(this IHostBuilder hostBuilder)
        {
            hostBuilder.RegisterSilkyServices<WebHostModule>();
            return hostBuilder;
        }
        
        public static IHostBuilder ConfigureSilkyWebHost<T>(this IHostBuilder hostBuilder) where T : StartUpModule
        {
            hostBuilder.RegisterSilkyServices<T>();
            return hostBuilder;
        }
    }
}