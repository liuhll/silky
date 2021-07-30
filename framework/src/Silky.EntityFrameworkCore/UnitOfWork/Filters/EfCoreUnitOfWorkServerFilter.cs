using System;
using System.Linq;
using Silky.Core;
using Silky.Core.Extensions;
using Silky.EntityFrameworkCore.ContextPool;
using Silky.Rpc.Runtime.Filters;
using Silky.Rpc.Runtime.Server.ContextPool;
using Silky.Rpc.Runtime.Server.UnitOfWork;

namespace Silky.EntityFrameworkCore.UnitOfWork
{
    public class EfCoreUnitOfWorkServerFilter : IServerFilter
    {
        private readonly IDbContextPool _dbContextPool;
        private UnitOfWorkAttribute _unitOfWorkAttribute;
        private bool _isManualSaveChanges;

        public EfCoreUnitOfWorkServerFilter()
        {
            _dbContextPool = EngineContext.Current.Resolve<IEfCoreDbContextPool>();
        }

        public int Order { get; } = Int32.MaxValue;

        public void OnActionExecuting(ServiceEntryExecutingContext context)
        {
            _unitOfWorkAttribute =
                context.ServiceEntry.CustomAttributes.OfType<UnitOfWorkAttribute>().FirstOrDefault();
            if (_unitOfWorkAttribute == null)
            {
                var instanceMethod = context.InstanceType?.GetCompareMethod(context.ServiceEntry.MethodInfo,
                    context.ServiceEntry.MethodInfo.Name);

                _unitOfWorkAttribute = instanceMethod?.GetCustomAttributes(true).OfType<UnitOfWorkAttribute>()
                    .FirstOrDefault();
            }

            _isManualSaveChanges = context.ServiceEntry.CustomAttributes.OfType<ManualCommitAttribute>().Any();

            if (_unitOfWorkAttribute != null)
            {
                // 开启事务
                _dbContextPool?.BeginTransaction(_unitOfWorkAttribute.EnsureTransaction);
            }
        }

        public void OnActionExecuted(ServiceEntryExecutedContext context)
        {
            try
            {
                if (_unitOfWorkAttribute == null)
                {
                    if (context.Exception == null && !_isManualSaveChanges) _dbContextPool?.SavePoolNow();
                }
                else
                {
                    _dbContextPool?.CommitTransaction(_isManualSaveChanges, context.Exception);
                }
            }
            finally
            {
                _dbContextPool?.CloseAll();
            }
        }
    }
}