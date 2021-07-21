using System.Threading.Tasks;
using Silky.Core.DynamicProxy;

namespace Silky.Transaction.Handler
{
    public interface ITransactionHandler
    {
        Task Handler(TransactionContext context, ISilkyMethodInvocation invocation);
    }
}