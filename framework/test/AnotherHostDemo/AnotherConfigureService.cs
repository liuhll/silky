using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Silky.Core.Modularity;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Silky.Codec;
using Silky.Core;
using Silky.SkyApm.Agent;
using Silky.Transaction.Repository.Redis;

namespace AnotherHostDemo
{
    // [DependsOn(/*typeof(MessagePackModule),*/
    //     typeof(SkyApmAgentModule))]
    public class AnotherConfigureService : IConfigureService
    {
        public ILogger<AnotherConfigureService> Logger { get; set; }

        public AnotherConfigureService()
        {
            Logger = NullLogger<AnotherConfigureService>.Instance;
        }

        public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            services.AddSilkySkyApm();
        }
    }
}