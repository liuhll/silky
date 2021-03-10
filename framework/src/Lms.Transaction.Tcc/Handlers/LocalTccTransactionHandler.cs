using System.Threading.Tasks;
using Lms.Core.DynamicProxy;
using Lms.Rpc.Transaction;

namespace Lms.Transaction.Tcc.Handlers
{
    public class LocalTccTransactionHandler : ITransactionHandler
    {
        public Task Handler(TransactionContext context, ILmsMethodInvocation invocation)
        {
            throw new System.NotImplementedException();
        }
    }
}