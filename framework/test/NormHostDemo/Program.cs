using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using SkyApm.Agent.GeneralHost;

namespace NormHostDemo
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            await CreateHostBuilder(args).Build().RunAsync();
        }

        private static IHostBuilder CreateHostBuilder(string[] args)
        {
            return Host.CreateDefaultBuilder(args)
                    .RegisterSilkyServices<NormHostDemoModule>()
                ;
        }
    }
}