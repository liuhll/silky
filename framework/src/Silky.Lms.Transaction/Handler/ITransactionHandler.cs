using System.Threading.Tasks;
using Silky.Lms.Core.DynamicProxy;

namespace Silky.Lms.Transaction.Handler
{
    public interface ITransactionHandler
    {
        Task Handler(TransactionContext context, ILmsMethodInvocation invocation);
    }
}