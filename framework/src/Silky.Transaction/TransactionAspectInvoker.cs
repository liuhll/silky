using System.Threading.Tasks;
using Silky.Core;
using Silky.Core.DynamicProxy;
using Silky.Rpc.Extensions;
using Silky.Transaction.Handler;
using Silky.Transaction.Abstraction;

namespace Silky.Transaction
{
    public class TransactionAspectInvoker
    {
        private static TransactionAspectInvoker invoker = new();

        private TransactionAspectInvoker()
        {
        }

        public static TransactionAspectInvoker GetInstance()
        {
            return invoker;
        }

        public async Task Invoke(TransactionContext transactionContext, ISilkyMethodInvocation invocation)
        {
            var transactionHandlerFactory = EngineContext.Current.Resolve<ITransactionHandlerFactory>();
            if (transactionHandlerFactory != null)
            {
                var serviceEntry = invocation.GetServiceEntry();
                var serviceKey = invocation.GetServiceKey();

                var transactionHandler =
                    transactionHandlerFactory.FactoryOf(transactionContext, serviceEntry, serviceKey);
                if (transactionHandler != null)
                {
                    await transactionHandler.Handler(transactionContext, invocation);
                }
                else
                {
                    await invocation.ProceedAsync();
                }
            }
            else
            {
                await invocation.ProceedAsync();
            }
        }
    }
}