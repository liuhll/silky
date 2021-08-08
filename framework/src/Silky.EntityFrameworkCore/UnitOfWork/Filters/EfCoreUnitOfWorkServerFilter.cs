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
        private readonly ISilkyDbContextPool _silkyDbContextPool;
        private UnitOfWorkAttribute _unitOfWorkAttribute;
        private bool _isManualSaveChanges;

        public EfCoreUnitOfWorkServerFilter()
        {
            _silkyDbContextPool = EngineContext.Current.Resolve<ISilkyDbContextPool>() as IEfCoreDbContextPool;
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
                _silkyDbContextPool?.BeginTransaction(_unitOfWorkAttribute.EnsureTransaction);
            }
        }

        public void OnActionExecuted(ServiceEntryExecutedContext context)
        {
            try
            {
                if (_unitOfWorkAttribute == null)
                {
                    if (context.Exception == null && !_isManualSaveChanges) _silkyDbContextPool?.SavePoolNow();
                }
                else
                {
                    _silkyDbContextPool?.CommitTransaction(_isManualSaveChanges, context.Exception);
                }
            }
            finally
            {
                _silkyDbContextPool?.CloseAll();
            }
        }
    }
}