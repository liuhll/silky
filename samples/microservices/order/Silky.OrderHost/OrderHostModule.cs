using System.Threading.Tasks;
using DotNetCore.CAP;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Silky.Codec;
using Silky.Core;
using Silky.Core.Modularity;
using Silky.Order.Application.Subscribe;
using Silky.Order.EntityFrameworkCore;
using Silky.SkyApm.Agent;

namespace Silky.OrderHost
{
    [DependsOn(typeof(SilkySkyApmAgentModule), typeof(MessagePackModule))]
    public class OrderHostModule : GeneralHostModule
    {
        public override void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            services.AddTransient<ISubscriberService, SubscriberService>();

            var rabbitMqOptions = configuration.GetSection("cap:rabbitmq").Get<RabbitMQOptions>();
            services.AddCap(x =>
            {
                x.UseEntityFramework<OrderDbContext>();
                x.UseRabbitMQ(z => { z = rabbitMqOptions; });
            });
            
                        
            services.AddDatabaseAccessor(
                options => { options.AddDb<OrderDbContext>(); },
                "Silky.Order.Database.Migrations");
        }

        public async override Task Initialize(ApplicationContext applicationContext)
        {
            if (EngineContext.Current.HostEnvironment.IsDevelopment() ||
                EngineContext.Current.HostEnvironment.IsEnvironment("ContainerDev"))
            {
                using var scope = applicationContext.ServiceProvider.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<OrderDbContext>();
                context.Database.Migrate();
            }
        }
    }
}