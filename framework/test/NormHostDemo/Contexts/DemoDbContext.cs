using Microsoft.EntityFrameworkCore;
using NormHostDemo.Tests;
using Silky.Lms.EntityFrameworkCore.Contexts;
using Silky.Lms.EntityFrameworkCore.Contexts.Attributes;

namespace NormHostDemo.Contexts
{
    [AppDbContext("DemoDbContext", DbProvider.MySql)]
    public class DemoDbContext : LmsDbContext<DemoDbContext>
    {
        public DemoDbContext(DbContextOptions<DemoDbContext> options) : base(options)
        {
        }

        public DbSet<Test> Tests { get; set; }
        
        
    }
}