using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Silky.Core;
using Silky.Core.Extensions;
using Silky.Core.Modularity;
using TestApplication.Contexts;

namespace TestApplication;

public class TestModel : SilkyModule
{
    public override Task Initialize(ApplicationContext applicationContext)
    {
        if (applicationContext.HostEnvironment.IsDevelopment())
        {
            using var scope = applicationContext.ServiceProvider.CreateScope();
            var demoDbContext = scope.ServiceProvider.GetRequiredService<DemoDbContext>();
            return demoDbContext.Database.MigrateAsync();
        }
        return Task.CompletedTask;
    }
}