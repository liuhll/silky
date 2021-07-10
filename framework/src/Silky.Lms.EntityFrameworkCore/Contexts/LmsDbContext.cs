using Microsoft.EntityFrameworkCore;

namespace Silky.Lms.EntityFrameworkCore.Contexts
{
    public abstract class LmsDbContext<TDbContext> : DbContext, IEfCoreDbContext
        where TDbContext : DbContext
    {
        protected LmsDbContext(DbContextOptions<TDbContext> options)
            : base(options)
        {

        }
    }
}