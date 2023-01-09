using System;
using System.Linq;
using System.Threading.Tasks;
using Silky.Core;
using Silky.Core.DbContext;
using Silky.Core.DbContext.UnitOfWork;
using Silky.Core.Extensions;
using Silky.Rpc.Filters;

namespace Silky.EntityFrameworkCore.UnitOfWork
{
    public class EfCoreUnitOfWorkServerFilter : IAsyncServerFilter, IOrderedFilter
    {
        private readonly ISilkyDbContextPool _silkyDbContextPool;
        private UnitOfWorkAttribute _unitOfWorkAttribute;
        private bool _isManualSaveChanges;

        public EfCoreUnitOfWorkServerFilter(ISilkyDbContextPool silkyDbContextPool)
        {
            _silkyDbContextPool = silkyDbContextPool;
        }

        public int Order { get; } = Int32.MaxValue - 1;
        
        public async Task OnActionExecutionAsync(ServerInvokeExecutingContext context, ServerExecutionDelegate next)
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

            var result = await next();
            try
            {
                if (_unitOfWorkAttribute == null)
                {
                    if (result.Exception == null && !_isManualSaveChanges) _silkyDbContextPool?.SavePoolNow();
                }
                else
                {
                    _silkyDbContextPool?.CommitTransaction(_isManualSaveChanges, result.Exception);
                }
            }
            finally
            {
                _silkyDbContextPool?.CloseAll();
            }
        }
    }
}