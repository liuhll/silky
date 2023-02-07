using System.Threading.Tasks;
using GatewayDemo.ClientFilters;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Silky.Core;

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
                   //.ConfigureWebHostDefaults(webBuilder =>
                   //{
                   //    webBuilder.UseStartup<Startup>()
                   //        .UseSerilogDefault();
                   //})
                   .ConfigureSilkyGatewayDefaults(webBuilder =>
                       {
                           webBuilder.UseStartup<Startup>()
                               .UseSerilogDefault();
                       },
                       options =>
                       {
                           options.ApplicationName = "SilkyGateway";
                           options.BannerMode = BannerMode.CONSOLE;
                           options.Filter.Clients.Add(new TestGlobalClientFilter());
                       })
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