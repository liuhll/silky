using System;
using System.Threading.Tasks;
using Silky.Core.DynamicProxy;
using Silky.Transaction.Handler;
using Silky.Transaction.Repository.Spi;
using Silky.Transaction.Tcc.Executor;

namespace Silky.Transaction.Tcc.Handlers
{
    public class StarterTccTransactionHandler : ITransactionHandler
    {
        private TccTransactionExecutor executor = TccTransactionExecutor.Executor;

        public async Task Handler(TransactionContext context, ISilkyMethodInvocation invocation)
        {
            var transaction = executor.PreTry(invocation);
            try
            {
                await invocation.ProceedAsync();
                transaction.Status = ActionStage.Trying;
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