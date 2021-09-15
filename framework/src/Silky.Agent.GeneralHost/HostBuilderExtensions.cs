using Silky.Core.Modularity;

namespace Microsoft.Extensions.Hosting
{
    public static class HostBuilderExtensions
    {
        public static IHostBuilder ConfigureSilkyGeneralHostDefaults(this IHostBuilder hostBuilder)
        {
            hostBuilder.RegisterSilkyServices<GeneralHostModule>();
            return hostBuilder;
        }
        
        public static IHostBuilder ConfigureSilkyGeneralHost<T>(this IHostBuilder hostBuilder) where T : StartUpModule
        {
            hostBuilder.RegisterSilkyServices<T>();
            return hostBuilder;
        }
    }
}