using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Silky.Core.Modularity;

namespace Silky.Account.EntityFrameworkCore;

public class AccountEfCoreModule : SilkyModule
{
    public override async Task Initialize(ApplicationInitializationContext context)
    {
        using var scope = context.ServiceProvider.CreateScope();
        var userDbContext = scope.ServiceProvider.GetRequiredService<UserDbContext>();
        await userDbContext.Database.MigrateAsync();
    }
}