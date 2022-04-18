using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NormHostDemo.Contexts;

namespace NormHostDemo
{
    public class NormHostConfigureService : IConfigureService
    {
        public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            services.AddDatabaseAccessor(options => { options.AddDbPool<DemoDbContext>(); }, "NormHostDemo");
            services.AddSilkySkyApm();
            services.AddJwt();
           // services.AddMessagePackCodec();
        }
    }
}