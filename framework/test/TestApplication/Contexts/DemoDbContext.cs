using Microsoft.EntityFrameworkCore;
using NormHostDemo.Tests;
using Silky.EntityFrameworkCore.Contexts;
using Silky.EntityFrameworkCore.Contexts.Attributes;
using Silky.EntityFrameworkCore.Extras.Contexts;

namespace TestApplication.Contexts
{
    [AppDbContext("DemoDbContext", DbProvider.MySql)]
    public class DemoDbContext : SilkyDbContext<DemoDbContext>
    {
        public DemoDbContext(DbContextOptions<DemoDbContext> options) : base(options)
        {
        }

        public DbSet<Test> Tests { get; set; }
    }
}