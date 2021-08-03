using Silky.Stock.Domain.Products;
using Microsoft.EntityFrameworkCore;
using Silky.EntityFrameworkCore.Contexts;
using Silky.EntityFrameworkCore.Contexts.Attributes;

namespace Silky.Stock.EntityFrameworkCore
{
    [AppDbContext("default", DbProvider.MySql)]
    public class StockDbContext : SilkyDbContext<StockDbContext>
    {
        public StockDbContext(DbContextOptions<StockDbContext> options)
            : base(options)
        {
        }

        public DbSet<Product> Products { get; set; }
    }
}