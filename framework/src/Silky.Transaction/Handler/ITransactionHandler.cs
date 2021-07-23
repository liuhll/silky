using System.Threading.Tasks;
using Silky.Core.DynamicProxy;
using Silky.Transaction.Repository.Spi;

namespace Silky.Transaction.Handler
{
    public interface ITransactionHandler
    {
        Task Handler(TransactionContext context, ISilkyMethodInvocation invocation);
    }
}