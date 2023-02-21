// See https://aka.ms/new-console-template for more information

using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using WebHostDemo;

var hostBuilder = Host.CreateDefaultBuilder(args)
    .ConfigureSilkyWebHostDefaults(webBuilder => webBuilder.UseStartup<Startup>());
await hostBuilder.RunConsoleAsync();