using Silky.Order.Application.Subscribe;
using Silky.Order.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Silky.Core;

namespace Silky.OrderHost
{
    public class CapConfigure : IConfigureService
    {
        public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            services.AddTransient<ISubscriberService,SubscriberService>();
            services.AddCap(x =>
            {
                x.UseEntityFramework<OrderDbContext>();
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