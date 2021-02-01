using System;
using System.Threading.Tasks;
using Lms.Core;
using Lms.Rpc;
using Microsoft.Extensions.Configuration;
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
//            Environment.SetEnvironmentVariable("DOTNET_ENVIRONMENT","test");
            
            return Host.CreateDefaultBuilder(args)
                    .RegisterLmsServices<ConsoleDemoModule>()
                    .ConfigureAppConfiguration((hosting,config) =>
                    {
                        
                      //  hosting.HostingEnvironment.EnvironmentName = Environments.Development;
                        // Adds JSON settings first
                        config.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                              .AddJsonFile($"appsettings.{hosting.HostingEnvironment.EnvironmentName}.json", optional: true);

                        // Adds YAML settings later
                        config.AddYamlFile("appsettings.yml", optional: true) 
                               .AddYamlFile($"appsettings.{hosting.HostingEnvironment.EnvironmentName}.yml", optional: true);
                        
                    })
                    
                    .UseRpcServer()
                    
                ;
            // .ConfigureWebHostDefaults(webBuilder =>
            //  {
            //      webBuilder.UseStartup<Startup>();
            //  });

        }
    }
}