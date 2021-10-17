using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;

namespace WebSocketDemo
{
    class Program
    {
        static async Task Main(string[] args)
        {
            await CreateHostBuilder(args).RunConsoleAsync();
        }
        
        private static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureSilkyWebSocketDefaults();
    }
}