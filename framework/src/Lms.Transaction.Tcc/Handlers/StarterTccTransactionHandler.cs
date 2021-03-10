using System.Threading.Tasks;
using Lms.Core.DynamicProxy;
using Lms.Rpc.Transaction;

namespace Lms.Transaction.Tcc.Handlers
{
    public class StarterTccTransactionHandler : ITransactionHandler
    {
        public Task Handler(TransactionContext context, ILmsMethodInvocation invocation)
        {
            return invocation.ProceedAsync();
        }
    }
}