using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Silky.Order.EntityFrameworkCore
{
    public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<OrderDbContext>
    {
        public OrderDbContext CreateDbContext(string[] args)
        {
            var configuration = BuildConfiguration();

            var builder = new DbContextOptionsBuilder<OrderDbContext>()
                .UseMySql(configuration.GetConnectionString("Default"),
                    ServerVersion.AutoDetect(configuration.GetConnectionString("Default")));
            return new OrderDbContext(builder.Options);
        }

        private static IConfiguration BuildConfiguration()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Path.Combine(Directory.GetCurrentDirectory(), "../Silky.OrderHost/"))
                .AddYamlFile("appsettings.Development.yml", optional: false);

            return builder.Build();
        }
    }
}