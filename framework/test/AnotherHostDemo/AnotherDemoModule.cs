using System.Threading.Tasks;
using Lms.Core.Modularity;
using Lms.DotNetty.Protocol.Ws;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace AnotherHostDemo
{
  
    [DependsOn(typeof(DotNettyHttpModule))]
    public class AnotherDemoModule : NormHostModule
    {
        public ILogger<AnotherDemoModule> Logger { get; set; } = NullLogger<AnotherDemoModule>.Instance;
        
        public async override Task Initialize(ApplicationContext applicationContext)
        {
            Logger.LogInformation("服务启动时执行方法");
        }

        public async override Task Shutdown(ApplicationContext applicationContext)
        {
            Logger.LogInformation("服务停止时执行的方法");
        }
    }
}