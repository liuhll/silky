using Microsoft.EntityFrameworkCore;
using NormHostDemo.Tests;
using Silky.EntityFrameworkCore.Contexts;
using Silky.EntityFrameworkCore.Contexts.Attributes;

namespace TestApplication.Contexts
{
    [AppDbContext("DemoDbContext", DbProvider.MySqlOfficial)]
    public class DemoDbContext : AbstractDbContext<DemoDbContext>
    {
        public DemoDbContext(DbContextOptions<DemoDbContext> options) : base(options)
        {
        }

        public DbSet<Test> Tests { get; set; }
    }
}