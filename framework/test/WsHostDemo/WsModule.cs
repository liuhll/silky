using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Silky.Core.Modularity;
using WsHostDemo.Contexts;

namespace WsHostDemo;

public class WsModule : PlugInModule
{
    public override Task Initialize(ApplicationInitializationContext context)
    {
        if (context.HostEnvironment.IsDevelopment())
        {
            // using var scope = context.ServiceProvider.CreateScope();
            var studentDbContext = context.ServiceProvider.GetRequiredService<StudentDbContext>();
            return studentDbContext.Database.MigrateAsync();
        }
        return Task.CompletedTask;
    }
    
}