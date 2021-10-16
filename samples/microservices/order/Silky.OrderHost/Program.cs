using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;

namespace Silky.OrderHost
{
    class Program
    {
        public static async Task Main(string[] args)
        {
            await CreateHostBuilder(args).Build().RunAsync();
        }

        private static IHostBuilder CreateHostBuilder(string[] args)
        {
            var hostBuilder = Host.CreateDefaultBuilder(args)
                .ConfigureSilkyGeneralHostDefaults()
                .UseSerilogDefault();
            if (hostBuilder.IsEnvironment("Apollo"))
            {
                hostBuilder.AddApollo();
            }

            return hostBuilder;
        }
    }
}