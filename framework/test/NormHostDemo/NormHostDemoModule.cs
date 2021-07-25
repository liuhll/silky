using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NormHostDemo.Contexts;
using Silky.Codec;
using Silky.Core.Modularity;
using Silky.SkyApm.Agent;

namespace NormHostDemo
{
    [DependsOn(typeof(MessagePackModule), typeof(SilkySkyApmAgentModule))]
    public class NormHostDemoModule : GeneralHostModule
    {
        public override void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            services.AddDatabaseAccessor(options => { options.AddDbPool<DemoDbContext>(); }, "NormHostDemo");
        }
    }
}