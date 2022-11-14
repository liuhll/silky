using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace SilkyAppHost
{
    class Program
    {
        public static async Task Main(string[] args)
        {
            await CreateHostBuilder(args).Build().RunAsync();
        }

#if (hosttype=="webhost")  
        private static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureSilkyWebHostDefaults(webBuilder => { webBuilder.UseStartup<Startup>(); });
#elif(hosttype=="websockethost")
        private static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureSilkyWebSocketDefaults();
#elif(hosttype=="gateway")
        private static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureSilkyGatewayDefaults(webBuilder => { webBuilder.UseStartup<Startup>(); });                
#else 
        private static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureSilkyGeneralHostDefaults();   
#endif
    }
}