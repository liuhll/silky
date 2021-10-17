using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;

namespace GeneralHostDemo
{
    class Program
    {
        static async Task Main(string[] args)
        {
            await CreateHostBuilder(args).RunConsoleAsync();
        }

        private static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureSilkyGeneralHostDefaults();
    }
}