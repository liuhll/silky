using DotNetCore.CAP;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Silky.Stock.EntityFrameworkCore;

namespace Silky.StockHost
{
    public class ConfigureService : IConfigureService
    {
        public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            services.AddSilkySkyApm();
            services.AddMessagePackCodec();
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
    }
}