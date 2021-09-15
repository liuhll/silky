using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace GatewayDemo
{
    public class Program
    {
        public async static Task Main(string[] args)
        {
            // LogManager.UseConsoleLogging(LogLevel.Debug);
            await CreateHostBuilder(args).Build().RunAsync();
        }

        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            var hostBuilder = Host.CreateDefaultBuilder(args)
                .ConfigureSilkyWebHostDefaults()
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>()
                        .UseSerilogDefault();
                });

            if (hostBuilder.IsEnvironment("Apollo"))
            {
                hostBuilder.AddApollo();
            }

            return hostBuilder;
        }
    }
}