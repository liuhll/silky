using Microsoft.EntityFrameworkCore;
using Silky.EntityFrameworkCore.Contexts;
using Silky.EntityFrameworkCore.Contexts.Attributes;

namespace Silky.Order.EntityFrameworkCore
{
    [AppDbContext("default",DbProvider.MySql)]
    public class OrderDbContext : SilkyDbContext<OrderDbContext>
    {
        public OrderDbContext(DbContextOptions<OrderDbContext> options) : base(options)
        {
        }

        public DbSet<Domain.Orders.Order> Orders { get; set; }
        
       
    }
}