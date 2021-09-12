using System;
using System.Linq;
using System.Threading.Tasks;
using Silky.Core.DynamicProxy;
using Silky.Rpc.Extensions;
using Silky.Transaction.Handler;
using Silky.Transaction.Abstraction;
using Silky.Transaction.Tcc.Diagnostics;
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
                var preTryInfo = await executor.PreTry(invocation);
                var transaction = preTryInfo.Item1;
                var transactionContext = preTryInfo.Item2;
                SilkyTransactionHolder.Instance.Set(transaction);
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