using System.Threading.Tasks;
using GatewayDemo.ClientFilters;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Serilog;
using Silky.Core;
using Silky.Core.Configuration;

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
                //.ConfigureWebHostDefaults(webBuilder => {
                //    webBuilder.UseStartup<Startup>()
                //        .UseSerilogDefault();
                //})
                .ConfigureSilkyGatewayDefaults(webBuilder =>
                    {
                        webBuilder.UseStartup<Startup>();
                    },
                    options =>
                    {
                        options.ApplicationName = "SilkyGateway";
                        options.BannerMode = BannerMode.CONSOLE;
                        // options.Filter.Clients.Add(new TestGlobalClientFilter());
                    })
                .UseSerilogDefault()
                   //.ConfigureSilkyGeneralHostDefaults();
                   ;

            if (hostBuilder.IsEnvironment("Apollo"))
            {
                hostBuilder.AddApollo();
                
            }

            return hostBuilder;
        }
    }
}