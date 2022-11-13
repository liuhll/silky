using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Silky.Core.Modularity;

namespace Silky.Stock.EntityFrameworkCore;

public class StockEfCoreModule : SilkyModule
{
    public override async Task Initialize(ApplicationInitializationContext context)
    {
        using var scope = context.ServiceProvider.CreateScope();
        var userDbContext = scope.ServiceProvider.GetRequiredService<StockDbContext>();
        await userDbContext.Database.MigrateAsync();
    }
}