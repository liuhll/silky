using System.Linq;
using System.Threading.Tasks;
using Silky.Core.DbContext;
using Silky.Core.DbContext.UnitOfWork;
using Silky.Core.Extensions;
using Silky.Rpc.Filters;

namespace Silky.EntityFrameworkCore.UnitOfWork
{
    public class EfCoreUnitOfWorkServerFilter : IAsyncServerFilter
    {
        private readonly ISilkyDbContextPool _silkyDbContextPool;

        public EfCoreUnitOfWorkServerFilter(ISilkyDbContextPool silkyDbContextPool)
        {
            _silkyDbContextPool = silkyDbContextPool;
        }

        public async Task OnActionExecutionAsync(ServerInvokeExecutingContext context, ServerExecutionDelegate next)
        {
            var instanceMethod = context.InstanceType?.GetCompareMethod(context.ServiceEntry.MethodInfo,
                context.ServiceEntry.MethodInfo.Name);
            var unitOfWorkAttribute =
                instanceMethod?.GetCustomAttributes(true).OfType<UnitOfWorkAttribute>().FirstOrDefault();

            var isManualSaveChanges = context.ServiceEntry.CustomAttributes.OfType<ManualCommitAttribute>().Any();

            if (unitOfWorkAttribute != null)
            {
                _silkyDbContextPool.BeginTransaction(unitOfWorkAttribute.EnsureTransaction);
            }
            var result = await next();
            try
            {
                if (unitOfWorkAttribute == null)
                {
                    if (result.Exception == null && !isManualSaveChanges)
                        await _silkyDbContextPool.SavePoolNowAsync();
                }
                else
                {
                    _silkyDbContextPool.CommitTransaction(isManualSaveChanges, result.Exception);
                }
            }
            finally
            {
                _silkyDbContextPool.CloseAll();
            }
        }
    }
}