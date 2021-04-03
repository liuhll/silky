using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Lms.Account.EntityFrameworkCore
{
    public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<UserDbContext>
    {
        public UserDbContext CreateDbContext(string[] args)
        {
            var configuration = BuildConfiguration();

            var builder = new DbContextOptionsBuilder<UserDbContext>()
                .UseMySql(configuration.GetConnectionString("Default"),
                    ServerVersion.AutoDetect(configuration.GetConnectionString("Default")));
            return new UserDbContext(builder.Options);
        }

        private static IConfiguration BuildConfiguration()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Path.Combine(Directory.GetCurrentDirectory(), "../Lms.AccountHost/"))
                .AddYamlFile("appsettings.yml", optional: false);

            return builder.Build();
        }
    }
}