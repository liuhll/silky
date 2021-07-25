using System.Threading.Tasks;
using Silky.Core.DynamicProxy;
using Silky.Transaction.Handler;
using Silky.Transaction.Repository.Spi;

namespace Silky.Transaction.Tcc.Handlers
{
    public class LocalTccTransactionHandler : ITransactionHandler
    {
        public async Task Handler(TransactionContext context, ISilkyMethodInvocation invocation)
        {
            await invocation.ProceedAsync();
        }
    }
}