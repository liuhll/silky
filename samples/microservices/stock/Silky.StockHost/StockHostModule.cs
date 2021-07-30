using System.Threading.Tasks;
using DotNetCore.CAP;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Silky.Core;
using Silky.Core.Modularity;
using Silky.SkyApm.Agent;
using Silky.Stock.EntityFrameworkCore;

namespace Silky.StockHost
{
    [DependsOn(typeof(SilkySkyApmAgentModule))]
    public class StockHostModule : GeneralHostModule
    {
        public override void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            var rabbitMqOptions = configuration.GetSection("cap:rabbitmq").Get<RabbitMQOptions>();
            services.AddCap(x =>
            {
                x.UseEntityFramework<StockDbContext>();
                x.UseRabbitMQ(z => { z = rabbitMqOptions; });
            });
            services.AddDatabaseAccessor(
                options => { options.AddDbPool<StockDbContext>(); },
                "Silky.Stock.Database.Migrations");
        }

        public async override Task Initialize(ApplicationContext applicationContext)
        {
            if (EngineContext.Current.HostEnvironment.IsDevelopment() || EngineContext.Current.HostEnvironment.EnvironmentName == "ContainerDev")
            {
                using var scope = applicationContext.ServiceProvider.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<StockDbContext>();
                context.Database.Migrate();
            }
        }
    }
}