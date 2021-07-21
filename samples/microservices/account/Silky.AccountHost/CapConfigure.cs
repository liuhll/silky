using Silky.Account.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Silky.Core;

namespace Silky.AccountHost
{
    public class CapConfigure : IConfigureService
    {
        public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            services.AddCap(x =>
            {
                x.UseEntityFramework<UserDbContext>();
                x.UseRabbitMQ(z =>
                {
                    z.HostName = "127.0.0.1";
                    z.UserName = "rabbitmq";
                    z.Password = "rabbitmq";
                });
            });
        }

        public int Order { get; } = 1;
    }
}