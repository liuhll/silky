using Microsoft.EntityFrameworkCore;
using Silky.Account.Domain.Accounts;
using Silky.EntityFrameworkCore.Contexts;
using Silky.EntityFrameworkCore.Contexts.Attributes;

namespace Silky.Account.EntityFrameworkCore
{
    [AppDbContext("default", DbProvider.MySql)]
    public class UserDbContext : SilkyDbContext<UserDbContext>
    {
        public UserDbContext(DbContextOptions<UserDbContext> options)
            : base(options)
        {
        }

        public DbSet<Domain.Accounts.Account> Accounts { get; set; }

        public DbSet<BalanceRecord> BalanceRecords { get; set; }
    }
}