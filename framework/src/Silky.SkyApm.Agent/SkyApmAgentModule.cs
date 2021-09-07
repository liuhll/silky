using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Silky.Core.Modularity;

namespace Silky.SkyApm.Agent
{
    public class SkyApmAgentModule : SilkyModule
    {
        public override void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            services.AddSilkySkyApm();
        }
    }
}