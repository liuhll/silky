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
            
            services.AddDatabaseAccessor(
                options => { options.AddDbPool<UserDbContext>(); },
                "Silky.Account.Database.Migrations");
            
        }
    }
}