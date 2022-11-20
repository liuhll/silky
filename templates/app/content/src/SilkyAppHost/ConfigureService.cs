using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
#if (typehost!="gateway")   
using SilkyApp.EntityFrameworkCore.DbContexts;
#endif

namespace SilkyAppHost
{
    public class ConfigureService : IConfigureService
    {
        public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            services.AddSilkySkyApm();

#if (typehost!="gateway")                  
            services.AddDatabaseAccessor(
                options => { options.AddDbPool<DefaultContext>(); },
                "SilkyApp.Database.Migrations");
#endif
        }
    }
}