using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Silky.Core;
using Silky.Core.MethodExecutor;
using Silky.Rpc.Runtime.Server.ContextPool;
using Silky.Rpc.Runtime.Server.UnitOfWork;

namespace Silky.Transaction.Tcc
{
    public static class ObjectMethodExecutorExtensions
    {
        [CanBeNull]
        public static async Task<object> ExecuteTccMethodAsync(this ObjectMethodExecutor executor, object target,
            object?[]? parameters)
        {
            object execResult;
            var dbContextPool = EngineContext.Current.Resolve<IDbContextPool>();
            var unitOfWorkAttribute = executor.MethodInfo.GetCustomAttributes().OfType<UnitOfWorkAttribute>()
                .FirstOrDefault();
            var isManualSaveChanges =
                executor.MethodInfo.GetCustomAttributes().OfType<ManualCommitAttribute>().Any();
            dbContextPool?.EnsureDbContextAddToPools();
            if (unitOfWorkAttribute != null)
            {
                dbContextPool?.BeginTransaction(unitOfWorkAttribute.EnsureTransaction);
            }
            try
            {
                if (executor.IsMethodAsync)
                {
                    execResult = await executor.ExecuteAsync(target, parameters);
                }
                else
                {
                    execResult = executor.Execute(target, parameters);
                }

                if (unitOfWorkAttribute == null)
                {
                    if (!isManualSaveChanges)
                    {
                        dbContextPool?.SavePoolNow();
                    }
                }
                else
                {
                    dbContextPool?.CommitTransaction(isManualSaveChanges);
                }
            }
            catch (Exception e)
            {
                dbContextPool?.CommitTransaction(isManualSaveChanges, e);
                throw;
            }
            finally
            {
                dbContextPool?.CloseAll();
            }
            return execResult;
        }
    }
}