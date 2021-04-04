using Microsoft.EntityFrameworkCore;

namespace Lms.Order.EntityFrameworkCore
{
    public class OrderDbContext : DbContext
    {
        public OrderDbContext(DbContextOptions<OrderDbContext> options) : base(options)
        {
        }

        public DbSet<Domain.Orders.Order> Orders { get; set; }
        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Domain.Orders.Order>();
        }
    }
}