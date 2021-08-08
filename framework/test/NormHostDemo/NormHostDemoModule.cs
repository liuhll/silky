using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NormHostDemo.Contexts;
using Silky.Core.Modularity;
using Silky.Jwt;
using Silky.ObjectMapper.Mapster;
using Silky.SkyApm.Agent;

namespace NormHostDemo
{
    [DependsOn( /*typeof(MessagePackModule),*/ typeof(SilkySkyApmAgentModule), typeof(MapsterModule),
        typeof(JwtModule))]
    public class NormHostDemoModule : GeneralHostModule
    {
        public override void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            services.AddDatabaseAccessor(options => { options.AddDb<DemoDbContext>(); }, "NormHostDemo");
        }

        public override async Task Initialize(ApplicationContext applicationContext)
        {
            using var scope = applicationContext.ServiceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<DemoDbContext>();
            await dbContext.Database.MigrateAsync();
        }
    }
}