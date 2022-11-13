using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Silky.Core.Modularity;

namespace Silky.Order.EntityFrameworkCore;

public class OrderEfCoreModule : SilkyModule
{
    public override async Task Initialize(ApplicationInitializationContext context)
    {
        using var scope = context.ServiceProvider.CreateScope();
        var userDbContext = scope.ServiceProvider.GetRequiredService<OrderDbContext>();
        await userDbContext.Database.MigrateAsync();
    }
}