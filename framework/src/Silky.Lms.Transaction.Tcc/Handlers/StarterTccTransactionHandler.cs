using System;
using System.Threading.Tasks;
using Silky.Lms.Core.DynamicProxy;
using Silky.Lms.Transaction.Handler;
using Silky.Lms.Transaction.Tcc.Executor;

namespace Silky.Lms.Transaction.Tcc.Handlers
{
    public class StarterTccTransactionHandler : ITransactionHandler
    {
        private TccTransactionExecutor executor = TccTransactionExecutor.Executor;

        public async Task Handler(TransactionContext context, ILmsMethodInvocation invocation)
        {
            var transaction = executor.PreTry(invocation);
            try
            {
                await invocation.ProceedAsync();
                transaction.Status = ActionStage.Trying;
                executor.UpdateStartStatus(transaction);
                await executor.GlobalConfirm(transaction);
            }
            catch (Exception e)
            {
                await executor.GlobalCancel(transaction);
                throw;
            }
        }
    }
}