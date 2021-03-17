using System.Threading.Tasks;
using Lms.Core.DynamicProxy;

namespace Lms.Transaction.Handler
{
    public interface ITransactionHandler
    {
        Task Handler(TransactionContext context, ILmsMethodInvocation invocation);
    }
}