using System.Threading.Tasks;
using Lms.Core;
using Microsoft.Extensions.Hosting;
namespace ConsoleDemo
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
                    .RegisterLmsServices<ConsoleDemoModule>()
                    //.UseRpcServer()
                ;
            // .ConfigureWebHostDefaults(webBuilder =>
            //  {
            //      webBuilder.UseStartup<Startup>();
            //  });

        }
    }
}