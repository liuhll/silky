using System.Threading.Tasks;
using Com.Ctrip.Framework.Apollo.Logging;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace GatewayDemo
{
    public class Program
    {
        public async static Task Main(string[] args)
        {
            LogManager.UseConsoleLogging(LogLevel.Debug);
            await CreateHostBuilder(args).Build().RunAsync();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .RegisterSilkyServices<GatewayHostModule>()
                .AddApollo()
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>()
                        .UseSerilogDefault();
                });
    }
}