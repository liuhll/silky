using System.Threading.Tasks;
using Silky.Core.Modularity;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Silky.Codec;
using Silky.SkyApm.Agent;
using Silky.Transaction.Repository.Redis;

namespace AnotherHostDemo
{
    [DependsOn(/*typeof(MessagePackModule),*/
        typeof(SilkySkyApmAgentModule))]
    public class AnotherDemoModule : WebSocketHostModule
    {
        public ILogger<AnotherDemoModule> Logger { get; set; }

        public AnotherDemoModule()
        {
            Logger = NullLogger<AnotherDemoModule>.Instance;
        }

        public async override Task Initialize(ApplicationContext applicationContext)
        {
            Logger.LogInformation("Execution method when the service starts");
        }

        public async override Task Shutdown(ApplicationContext applicationContext)
        {
            Logger.LogInformation("Method to be executed when the service is stopped");
        }
    }
}