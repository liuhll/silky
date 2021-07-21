using Microsoft.EntityFrameworkCore;

namespace Silky.Account.EntityFrameworkCore
{
    public class UserDbContext : DbContext
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