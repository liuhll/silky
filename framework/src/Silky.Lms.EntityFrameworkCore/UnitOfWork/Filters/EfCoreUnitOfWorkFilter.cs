using System.Threading.Tasks;
using Silky.Lms.Core.DynamicProxy;
using Silky.Lms.EntityFrameworkCore.ContextPools;

namespace Silky.Lms.EntityFrameworkCore.UnitOfWork
{
    public class EfCoreUnitOfWorkFilter
    {
        private readonly IDbContextPool _dbContextPool;

        public EfCoreUnitOfWorkFilter(IDbContextPool dbContextPool)
        {
            _dbContextPool = dbContextPool;
        }

        public async Task InterceptAsync(ILmsMethodInvocation invocation)
        {
        }
    }
}