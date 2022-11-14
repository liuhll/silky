using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
#if (hosttype!="gateway")  
using SilkyApp.EntityFrameworkCore.DbContexts;
#endif

namespace SilkyAppHost
{
    public class ConfigureService : IConfigureService
    {
        public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            services.AddSilkySkyApm();
                
#if (hosttype !="gateway")                  
            services.AddDatabaseAccessor(
                options => { options.AddDbPool<DefaultContext>(); },
                "SilkyApp.Database.Migrations");
#endif
        }
    }
}