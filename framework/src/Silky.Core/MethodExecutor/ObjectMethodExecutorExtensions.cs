using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Silky.Core.DbContext;
using Silky.Core.DbContext.UnitOfWork;

namespace Silky.Core.MethodExecutor
{
    public static class ObjectMethodExecutorExtensions
    {
        [CanBeNull]
        public static async Task<object> ExecuteMethodWithDbContextAsync([NotNull] this ObjectMethodExecutor executor,
            object target,
            object?[]? parameters)
        {
            Check.NotNull(executor, nameof(executor));
            Check.NotNull(target, nameof(target));
            object execResult;
            var dbContextPool = EngineContext.Current.Resolve<ISilkyDbContextPool>();
            var unitOfWorkAttribute =
                executor.MethodInfo.GetCustomAttributes().OfType<UnitOfWorkAttribute>().FirstOrDefault();
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