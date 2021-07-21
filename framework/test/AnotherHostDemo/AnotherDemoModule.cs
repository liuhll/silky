using System.Threading.Tasks;
using Silky.Core.Modularity;
using Silky.WebSocket;
using Silky.Agent.WebSocketHost;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace AnotherHostDemo
{
    public class AnotherDemoModule : WebSocketHostModule
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