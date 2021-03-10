using System.Threading.Tasks;
using Lms.Core.DynamicProxy;

namespace Lms.Rpc.Transaction
{
    public interface ITransactionHandler
    {
        Task Handler(TransactionContext context, ILmsMethodInvocation invocation);
    }
}