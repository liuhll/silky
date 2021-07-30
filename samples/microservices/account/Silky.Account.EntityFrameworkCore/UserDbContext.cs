using Microsoft.EntityFrameworkCore;
using Silky.EntityFrameworkCore.Contexts;
using Silky.EntityFrameworkCore.Contexts.Attributes;

namespace Silky.Account.EntityFrameworkCore
{
    [AppDbContext("default",DbProvider.MySql)]
    public class UserDbContext : SilkyDbContext<UserDbContext>
    {
        public UserDbContext(DbContextOptions<UserDbContext> options)
            : base(options)
        {
        }
        
        public DbSet<Domain.Accounts.Account> Accounts { get; set; }
        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Domain.Accounts.Account>();
            modelBuilder.Entity<Domain.Accounts.BalanceRecord>();
        }
    }
}