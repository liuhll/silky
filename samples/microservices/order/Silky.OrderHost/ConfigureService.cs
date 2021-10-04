using DotNetCore.CAP;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Silky.Order.Application.Subscribe;
using Silky.Order.EntityFrameworkCore;

namespace Silky.OrderHost
{
    public class ConfigureService : IConfigureService
    {
        public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            
            services.AddSilkySkyApm();
            services.AddMessagePackCodec();
            services.AddTransient<ISubscriberService, SubscriberService>();

            var rabbitMqOptions = configuration.GetSection("cap:rabbitmq").Get<RabbitMQOptions>();
            services.AddCap(x =>
            {
                x.UseEntityFramework<OrderDbContext>();
                x.UseRabbitMQ(z => { z = rabbitMqOptions; });
            });
            
                        
            services.AddDatabaseAccessor(
                options => { options.AddDbPool<OrderDbContext>(); },
                "Silky.Order.Database.Migrations");
        }
    }
}