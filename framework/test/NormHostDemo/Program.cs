using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Silky.Core;

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
            var hostBuilder = Host
                    .CreateDefaultBuilder(args)
                    .ConfigureSilkyGeneralHostDefaults(options =>
                    {
                        options.DisplayFullErrorStack = true;
                        options.ApplicationName = "NormHost";
                        options.BannerMode = BannerMode.CONSOLE;
#if DEBUG
                        options.IgnoreCheckingRegisterType = true;
#endif
                    })
                    .UseSerilogDefault()
                ;
            if (hostBuilder.IsEnvironment("Apollo"))
            {
                hostBuilder.AddApollo();
            }

            return hostBuilder;
        }
    }
}