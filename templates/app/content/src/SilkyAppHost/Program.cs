using Microsoft.Extensions.Hosting;
#if (type=="webhost" || type=="gateway")
using Microsoft.AspNetCore.Hosting;
using SilkyAppHost;
#endif  

var hostBuilder = Host.CreateDefaultBuilder()
#if (type=="webhost") 
    .ConfigureSilkyWebHostDefaults(webBuilder => { webBuilder.UseStartup<Startup>(); })
#elif(type=="wshost")
    .ConfigureSilkyWebSocketDefaults()
#elif(type=="gateway")
    .ConfigureSilkyGatewayDefaults(webBuilder => { webBuilder.UseStartup<Startup>(); })
#else
    .ConfigureSilkyGeneralHostDefaults()
#endif
    ;
await hostBuilder.Build().RunAsync();
