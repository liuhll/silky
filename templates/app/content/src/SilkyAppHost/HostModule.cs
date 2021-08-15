using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Silky.Codec;
using Silky.Core;
using Silky.Core.Modularity;
using Silky.SkyApm.Agent;
using SilkyApp.EntityFrameworkCore.DbContexts;

namespace SilkyAppHost
{
    [DependsOn(typeof(SilkySkyApmAgentModule),
        typeof(MessagePackModule))]
#if !supportwebsocket
    public class HostModule : GeneralHostModule
#else
    public class HostModule : WebSocketHostModule
#endif
    {
        public override void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            services.AddDatabaseAccessor(
                options => { options.AddDbPool<DefaultContext>(); },
                "SilkyApp.Database.Migrations");
        }

        public async override Task Initialize(ApplicationContext applicationContext)
        {
            if (EngineContext.Current.HostEnvironment.IsDevelopment()
                || EngineContext.Current.HostEnvironment.EnvironmentName.Contains("dev",
                    StringComparison.OrdinalIgnoreCase))
            {
                using var scope = applicationContext.ServiceProvider.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<DefaultContext>();
                context.Database.Migrate();
            }
        }
    }
}