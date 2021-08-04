using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Silky.Core.Modularity;
using Silky.Rpc.SkyApm.Diagnostics;
using Silky.SkyApm.Agent.Configuration;
using SkyApm.Utilities.Configuration;

namespace Silky.SkyApm.Agent
{
    public class SilkySkyApmAgentModule : SilkyModule
    {
        public override void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
           
            services.AddSkyAPM(extensions =>
            {
                extensions.AddSilkyRpc();
                extensions.Services.AddSingleton<IConfigurationFactory, SilkyConfigurationFactory>();
            });
        }
    }
}