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
    }
}