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
    public class EfCoreUnitOfWorkServerFilter : IAsyncServerFilter
    {
        public async Task OnActionExecutionAsync(ServerInvokeExecutingContext context, ServerExecutionDelegate next)
        {
            var instanceMethod = context.InstanceType?.GetCompareMethod(context.ServiceEntry.MethodInfo,
                context.ServiceEntry.MethodInfo.Name);
            var unitOfWorkAttribute =
                instanceMethod?.GetCustomAttributes(true).OfType<UnitOfWorkAttribute>().FirstOrDefault();

            var isManualSaveChanges = context.ServiceEntry.CustomAttributes.OfType<ManualCommitAttribute>().Any();
            var silkyDbContextPool = EngineContext.Current.Resolve<ISilkyDbContextPool>();

            if (unitOfWorkAttribute != null)
            {
                silkyDbContextPool.BeginTransaction(unitOfWorkAttribute.EnsureTransaction);
            }

            var result = await next();
            try
            {
                if (unitOfWorkAttribute == null)
                {
                    if (result.Exception == null && !isManualSaveChanges)
                        await silkyDbContextPool.SavePoolNowAsync();
                }
                else
                {
                    if (result.Exception == null)
                    {
                        silkyDbContextPool.CommitTransaction();
                    }
                    else
                    {
                        silkyDbContextPool.RollbackTransaction();
                    }
                }
            }
            catch (Exception ex)
            {
               if (unitOfWorkAttribute != null) 
               {
                    silkyDbContextPool.RollbackTransaction();
               }
               throw;
            }
            finally
            {
                silkyDbContextPool.CloseAll();
            }
        }
    }
}