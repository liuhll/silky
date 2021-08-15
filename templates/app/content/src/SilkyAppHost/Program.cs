using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;

namespace SilkyAppHost
{
    class Program
    {
        public static async Task Main(string[] args)
        {
            await CreateHostBuilder(args).Build().RunAsync();
        }

        private static IHostBuilder CreateHostBuilder(string[] args)
        {
#if (dotnetenv=="Apollo")
            return Host.CreateDefaultBuilder(args)
                .RegisterSilkyServices<HostModule>()
                .AddApollo();

#else
            return Host.CreateDefaultBuilder(args)
                .RegisterSilkyServices<HostModule>();
#endif
        }
    }
}