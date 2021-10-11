using System;
using System.Linq;
using Silky.Core;
using Silky.Core.DbContext;
using Silky.Core.DbContext.UnitOfWork;
using Silky.Core.DependencyInjection;
using Silky.Core.Extensions;
using Silky.Rpc.Runtime.Server;

namespace Silky.EntityFrameworkCore.UnitOfWork
{
    public class EfCoreUnitOfWorkServerFilter : IServerFilter, IScopedDependency
    {
        private readonly ISilkyDbContextPool _silkyDbContextPool;
        private UnitOfWorkAttribute _unitOfWorkAttribute;
        private bool _isManualSaveChanges;

        public EfCoreUnitOfWorkServerFilter()
        {
            _silkyDbContextPool = EngineContext.Current.Resolve<ISilkyDbContextPool>();
        }

        public int Order { get; } = Int32.MaxValue;

        public void OnActionExecuting(ServerExecutingContext context)
        {
            var instanceMethod = context.InstanceType?.GetCompareMethod(context.ServiceEntry.MethodInfo,
                context.ServiceEntry.MethodInfo.Name);
            _unitOfWorkAttribute =
                instanceMethod?.GetCustomAttributes(true).OfType<UnitOfWorkAttribute>().FirstOrDefault();

            _isManualSaveChanges = context.ServiceEntry.CustomAttributes.OfType<ManualCommitAttribute>().Any();

            if (_unitOfWorkAttribute != null)
            {
                _silkyDbContextPool?.BeginTransaction(_unitOfWorkAttribute.EnsureTransaction);
            }
        }

        public void OnActionExecuted(ServerExecutedContext context)
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