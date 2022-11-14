using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Silky.Core;
using Silky.Core.Configuration;
using Silky.Core.Modularity.PlugIns;

namespace GatewayDemo
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            await CreateHostBuilder(args).Build().RunAsync();
        }

        private static IHostBuilder CreateHostBuilder(string[] args)
        {
            var hostBuilder = Host.CreateDefaultBuilder(args)
                .ConfigureSilkyGatewayDefaults(webBuilder =>
                    {
                        webBuilder.UseStartup<Startup>()
                            .UseSerilogDefault();
                    },
                    options =>
                    {
                        options.ApplicationName = "SilkyGateway";
                        options.BannerMode = BannerMode.CONSOLE;
                    });

            if (hostBuilder.IsEnvironment("Apollo"))
            {
                hostBuilder.AddApollo();
            }

            return hostBuilder;
        }
    }
}