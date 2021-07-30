using System.Threading.Tasks;
using DotNetCore.CAP;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Silky.Account.EntityFrameworkCore;
using Silky.Core;
using Silky.Core.Modularity;

namespace Silky.AccountHost
{
    public class AccountHostModule : GeneralHostModule
    {
        public override void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
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

        public async override Task Initialize(ApplicationContext applicationContext)
        {
            if (EngineContext.Current.HostEnvironment.IsDevelopment() ||
                EngineContext.Current.HostEnvironment.EnvironmentName == "ContainerDev")
            {
                using var scope = applicationContext.ServiceProvider.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<UserDbContext>();
                context.Database.Migrate();
            }
        }
    }
}