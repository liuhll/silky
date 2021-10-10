using Silky.SkyApm.Agent.Configuration;
using Silky.SkyApm.Diagnostics.Http;
using Silky.SkyApm.Diagnostics.Rpc;
using Silky.SkyApm.Diagnostics.Transaction;
using SkyApm.Utilities.Configuration;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class SkyApmAgentCollectionExtensions
    {
        public static IServiceCollection AddSilkySkyApm(this IServiceCollection services)
        {
            services.AddSkyAPM(extensions =>
            {
                extensions.AddSkyApmSilkyRpc();
                extensions.AddSkyApmSilkyHttp();
                extensions.AddSkyApmSilkyTransaction();
                extensions.Services.AddSingleton<IConfigurationFactory, SilkyConfigurationFactory>();
            });
            return services;
        }
    }
}