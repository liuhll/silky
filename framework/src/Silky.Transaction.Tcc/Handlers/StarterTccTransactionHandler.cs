using System;
using System.Threading.Tasks;
using Silky.Core.DynamicProxy;
using Silky.Transaction.Handler;
using Silky.Transaction.Abstraction;
using Silky.Transaction.Tcc.Executor;

namespace Silky.Transaction.Tcc.Handlers
{
    public class StarterTccTransactionHandler : ITransactionHandler
    {
        private TccTransactionExecutor executor = TccTransactionExecutor.Executor;
        
        public async Task Handler(TransactionContext context, ISilkyMethodInvocation invocation)
        {
            try
            {
                var transaction = await executor.PreTry(invocation);
                SilkyTransactionHolder.Instance.Set(transaction);
                var transactionContext = new TransactionContext
                {
                    Action = ActionStage.Trying,
                    TransId = transaction.TransId,
                    TransactionRole = TransactionRole.Start,
                    TransType = TransactionType.Tcc
                };
                SilkyTransactionContextHolder.Instance.Set(transactionContext);
                try
                {
                    await invocation.ProceedAsync();
                    transaction.Status = ActionStage.Trying;
                    await executor.UpdateStartStatus(transaction);
                }
                catch (Exception e)
                {
                    var errorCurrentTransaction = SilkyTransactionHolder.Instance.CurrentTransaction;
                    await executor.GlobalCancel(errorCurrentTransaction);
                    throw;
                }

                var currentTransaction = SilkyTransactionHolder.Instance.CurrentTransaction;
                await executor.GlobalConfirm(currentTransaction);
            }
            finally
            {
                SilkyTransactionContextHolder.Instance.Remove();
                executor.Remove();
            }
        }
    }
}