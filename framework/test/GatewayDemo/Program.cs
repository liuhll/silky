using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

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
                // .ConfigureSilkyGatewayDefaults(webBuilder =>
                // {
                //     webBuilder.UseStartup<Startup>()
                //         .UseSerilogDefault();
                // });
                .ConfigureSilkyWebHostDefaults(webBuilder =>
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