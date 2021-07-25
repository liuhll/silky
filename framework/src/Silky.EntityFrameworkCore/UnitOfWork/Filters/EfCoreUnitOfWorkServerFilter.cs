using System;
using System.Linq;
using Silky.EntityFrameworkCore.ContextPools;
using Silky.Rpc.Runtime.Filters;
using Silky.Rpc.Runtime.Server.UnitOfWork;

namespace Silky.EntityFrameworkCore.UnitOfWork
{
    public class EfCoreUnitOfWorkServerFilter : IServerFilter
    {
        private readonly IDbContextPool _dbContextPool;
        private UnitOfWorkAttribute _unitOfWorkAttribute;
        private bool _isManualSaveChanges;

        public EfCoreUnitOfWorkServerFilter(IDbContextPool dbContextPool)
        {
            _dbContextPool = dbContextPool;
        }

        public int Order { get; } = Int32.MaxValue;

        public void OnActionExecuting(ServiceEntryExecutingContext context)
        {
            _unitOfWorkAttribute =
                context.ServiceEntry.CustomAttributes.OfType<UnitOfWorkAttribute>().FirstOrDefault();
            _isManualSaveChanges = context.ServiceEntry.CustomAttributes.OfType<ManualCommitAttribute>().Any();

            if (_unitOfWorkAttribute != null)
            {
                // 开启事务
                _dbContextPool.BeginTransaction(_unitOfWorkAttribute.EnsureTransaction);
            }
        }

        public void OnActionExecuted(ServiceEntryExecutedContext context)
        {
            try
            {
                if (_unitOfWorkAttribute == null)
                {
                    if (context.Exception == null && !_isManualSaveChanges) _dbContextPool.SavePoolNow();
                }
                else
                {
                    _dbContextPool.CommitTransaction(_isManualSaveChanges, context.Exception);
                }
            }
            finally
            {
                _dbContextPool.CloseAll();
            }
        }
    }
}