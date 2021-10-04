using DotNetCore.CAP;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Silky.Account.EntityFrameworkCore;

namespace Silky.AccountHost
{
    public class ConfigureService : IConfigureService
    {
        public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            services.AddSilkySkyApm();
            services.AddJwt();
            services.AddMessagePackCodec();
            
            var rabbitMqOptions = configuration.GetSection("cap:rabbitmq").Get<RabbitMQOptions>();
            services.AddCap(x =>
            {
                x.UseEntityFramework<UserDbContext>();
                x.UseRabbitMQ(z => { z = rabbitMqOptions; });
            });
            services.AddDatabaseAccessor(
                options => { options.AddDbPool<UserDbContext>(); },
                "Silky.Account.Database.Migrations");
            
        }
    }
}