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
        public static async Task<object?> ExecuteMethodWithDbContextAsync([NotNull] this ObjectMethodExecutor executor,
            [NotNull] object target,
            object?[]? parameters)
        {
            Check.NotNull(executor, nameof(executor));
            Check.NotNull(target, nameof(target));

            object? execResult = null;
            var dbContextPool = EngineContext.Current.Resolve<ISilkyDbContextPool>();

            if (dbContextPool == null)
            {
                if (executor.IsMethodAsync)
                {
                    execResult = await executor.ExecuteAsync(target, parameters);
                }
                else
                {
                    execResult = executor.Execute(target, parameters);
                }

                return execResult;
            }

            var unitOfWorkAttribute =
                executor.MethodInfo.GetCustomAttributes().OfType<UnitOfWorkAttribute>().FirstOrDefault();
            var isManualSaveChanges =
                executor.MethodInfo.GetCustomAttributes().OfType<ManualCommitAttribute>().Any();
            if (unitOfWorkAttribute != null)
            {
                dbContextPool.BeginTransaction(unitOfWorkAttribute.EnsureTransaction);
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
                        await dbContextPool.SavePoolNowAsync();
                    }
                }
                else
                {
                    dbContextPool.CommitTransaction(isManualSaveChanges);
                }
            }
            catch (Exception e)
            {
                dbContextPool.CommitTransaction(isManualSaveChanges, e);
                throw;
            }
            finally
            {
                dbContextPool.CloseAll();
            }

            return execResult;
        }
    }
}