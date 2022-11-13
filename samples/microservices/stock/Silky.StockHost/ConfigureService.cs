
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
            services.AddDatabaseAccessor(
                options => { options.AddDbPool<StockDbContext>(); },
                "Silky.Stock.Database.Migrations");
        }
    }
}