using System.Threading.Tasks;
using Silky.Core.DynamicProxy;
using Silky.Transaction.Abstraction;

namespace Silky.Transaction.Handler
{
    public interface ITransactionHandler
    {
        TransactionRole TransactionRole { get; }

        Task Handler(TransactionContext context, ISilkyMethodInvocation invocation);
    }
}