using System.Threading.Tasks;
using Silky.Core.DynamicProxy;
using Silky.Transaction.Handler;
using Silky.Transaction.Tcc.Executor;

namespace Silky.Transaction.Tcc.Handlers
{
    public class ParticipantTccTransactionHandler : ITransactionHandler
    {
        private readonly TccTransactionExecutor _executor = TccTransactionExecutor.Executor;

        public Task Handler(TransactionContext context, ISilkyMethodInvocation invocation)
        {
            return invocation.ProceedAsync();
        }
    }
}