using System.Threading.Tasks;
using Lms.Core.DynamicProxy;
using Lms.Rpc.Transaction;
using Lms.Transaction.Tcc.Executor;

namespace Lms.Transaction.Tcc.Handlers
{
    public class StarterTccTransactionHandler : ITransactionHandler
    {
        private TccTransactionExecutor executor = TccTransactionExecutor.Executor;
        public async Task Handler(TransactionContext context, ILmsMethodInvocation invocation)
        {
            var transaction = executor.PreTry(invocation);
            await invocation.ProceedAsync();
            transaction.Status = ActionStage.Trying;
            executor.UpdateStartStatus(transaction);
            await  executor.GlobalConfirm(transaction);
        }
    }
}