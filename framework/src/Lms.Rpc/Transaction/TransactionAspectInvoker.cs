using System.Threading.Tasks;
using Lms.Core;
using Lms.Core.DynamicProxy;

namespace Lms.Rpc.Transaction
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

        public async Task Invoke(TransactionContext transactionContext, ILmsMethodInvocation invocation)
        {
            var transactionHandlerFactory = EngineContext.Current.Resolve<ITransactionHandlerFactory>();
            if (transactionHandlerFactory != null)
            {
                var transactionHandler = transactionHandlerFactory.FactoryOf(transactionContext);
                await transactionHandler.Handler(transactionContext, invocation);
            }
            else
            {
                await invocation.ProceedAsync();
            }

        }
    }
}