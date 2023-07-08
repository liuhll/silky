using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Silky.Core.Modularity;
using Silky.Core.Modularity.PlugIns;
using TestApplication.Contexts;

namespace TestApplication;

public class TestModule : PlugInModule
{
    public override Task Initialize(ApplicationInitializationContext context)
    {
        if (context.HostEnvironment.IsDevelopment())
        {
            // using var scope = context.ServiceProvider.CreateScope();
            var demoDbContext = context.ServiceProvider.GetRequiredService<DemoDbContext>();
            return demoDbContext.Database.MigrateAsync();
        }
        return Task.CompletedTask;
    }

    public override void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddDatabaseAccessor(options => { options.AddDbPool<DemoDbContext>(); }, "TestApplication");
    }
}