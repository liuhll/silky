using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace SilkyApp.GatewayHost
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            await CreateHostBuilder(args).Build().RunAsync();
        }

        private static IHostBuilder CreateHostBuilder(string[] args)
        {
#if (dotnetenv=="Apollo")
            return Host.CreateDefaultBuilder(args)
                .RegisterSilkyServices<GatewayHostModule>()
                .ConfigureWebHostDefaults(webBuilder => { webBuilder.UseStartup<Startup>(); })
                .AddApollo();

#else
            return Host.CreateDefaultBuilder(args)
                .RegisterSilkyServices<GatewayHostModule>()
                .ConfigureWebHostDefaults(webBuilder => { webBuilder.UseStartup<Startup>(); });
#endif
        }
    }
}