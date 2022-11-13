using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Silky.Order.EntityFrameworkCore;

namespace Silky.OrderHost
{
    public class ConfigureService : IConfigureService
    {
        public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            
            services.AddSilkySkyApm();
            services.AddDatabaseAccessor(
                options => { options.AddDbPool<OrderDbContext>(); },
                "Silky.Order.Database.Migrations");
        }
    }
}