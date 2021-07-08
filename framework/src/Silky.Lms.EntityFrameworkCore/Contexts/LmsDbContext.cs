using Microsoft.EntityFrameworkCore;

namespace Silky.Lms.EntityFrameworkCore.Contexts
{
    public abstract class LmsDbContext<TDbContext> : DbContext
        where TDbContext : DbContext
    {
    }
}