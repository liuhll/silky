using System;
using Silky.Lms.EntityFrameworkCore.ContextPools;
using Silky.Lms.Rpc.Runtime.Server.Filters;

namespace Silky.Lms.EntityFrameworkCore.UnitOfWork
{
    public class EfCoreUnitOfWorkFilter : IServiceEntryFilter
    {
        private readonly IDbContextPool _dbContextPool;

        public EfCoreUnitOfWorkFilter(IDbContextPool dbContextPool)
        {
            _dbContextPool = dbContextPool;
        }

        public int Order { get; } = Int32.MaxValue;

        public void OnActionExecuting(ServiceEntryExecutingContext context)
        {
            // var uow = context.ServiceEntry.CustomAttributes.OfType<UnitOfWorkAttribute>().FirstOrDefault();
            // _dbContextPool.BeginTransaction(uow?.EnsureTransaction ?? false);
        }

        public void OnActionExecuted(ServiceEntryExecutedContext context)
        {
            //_dbContextPool.CommitTransaction();
            _dbContextPool.SavePoolNow();
        }
    }
}