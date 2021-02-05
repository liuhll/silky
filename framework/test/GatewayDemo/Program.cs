using System.Threading.Tasks;
using Lms.Core;
using Lms.Rpc;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace GatewayDemo
{
    public class Program
    {
        public async static Task Main(string[] args)
        {
            await CreateHostBuilder(args).Build().RunAsync();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .RegisterLmsServices<GatewayDemoModule>()
                 .ConfigureAppConfiguration((hosting,config) =>
                 {
                         
                     //  hosting.HostingEnvironment.EnvironmentName = Environments.Development;
                     // Adds JSON settings first
                     // config.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                     //     .AddJsonFile($"appsettings.{hosting.HostingEnvironment.EnvironmentName}.json", optional: true);
                 
                     // Adds YAML settings later
                     config.AddYamlFile("appsettings.yml", optional: true) 
                         .AddYamlFile($"appsettings.{hosting.HostingEnvironment.EnvironmentName}.yml", optional: true);
                         
                 })
                .UseRpcServer()
                .ConfigureWebHostDefaults(webBuilder => { webBuilder.UseStartup<Startup>(); });
    }
}