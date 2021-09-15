using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NormHostDemo.Contexts;
using Silky.Core;
using Silky.Core.Modularity;
using Silky.Jwt;
using Silky.ObjectMapper.Mapster;
using Silky.SkyApm.Agent;

namespace NormHostDemo
{
    public class NormHostConfigureService : IConfigureService
    {
        public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            services.AddDatabaseAccessor(options => { options.AddDbPool<DemoDbContext>(); }, "NormHostDemo");
            services.AddSilkySkyApm();
            services.AddJwt();
        }
    }
}