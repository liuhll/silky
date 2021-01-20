using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace WebApplication
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
           var hostBuilder = new WebHostBuilder()
                  .UseContentRoot(Directory.GetCurrentDirectory())
                  .UseKestrel((context,options) =>
                  {
                      // options.Limits.MaxConcurrentConnections = 100;
                      // options.Limits.MaxConcurrentUpgradedConnections = 100;
                      // options.Limits.MaxRequestBodySize = 10 * 1024;
                      // options.Limits.MinRequestBodyDataRate =
                      //     new MinDataRate(bytesPerSecond: 100, 
                      //         gracePeriod: TimeSpan.FromSeconds(10));
                      // options.Limits.MinResponseDataRate =
                      //     new MinDataRate(bytesPerSecond: 100, 
                      //         gracePeriod: TimeSpan.FromSeconds(10));
                      options.Listen(IPAddress.Parse("127.0.0.1"), 8089, listenOptions =>
                      {
                          listenOptions.Protocols = HttpProtocols.Http1AndHttp2;
                      });

                  })
                  .ConfigureServices(services =>
                  {
                      services.AddMvc();
                     
                  })
                  .ConfigureLogging((logger) =>
                  {
                      logger.AddConsole();
                  })
                  .Configure(app =>
                  {
                      // app.UseStaticFiles();
                      // app.UseMvc(route =>
                      // {
                      //     
                      // });
                      app.UseRouting();
                      app.Run( async context =>
                      {
                          await context.Response.WriteAsync("OK");
                      });
                  });

                var host = hostBuilder.Build();
                await host.RunAsync();
        }

       
    }
}