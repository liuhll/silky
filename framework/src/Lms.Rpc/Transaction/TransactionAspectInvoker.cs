using System.Threading.Tasks;
using Lms.Core;
using Lms.Core.DynamicProxy;
using Lms.Rpc.Runtime.Server;

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
                var serviceEntry = invocation.ArgumentsDictionary["serviceEntry"] as ServiceEntry;
                var serviceKey = invocation.ArgumentsDictionary["serviceKey"] as string;

                var transactionHandler =
                    transactionHandlerFactory.FactoryOf(transactionContext, serviceEntry, serviceKey);
                await transactionHandler.Handler(transactionContext, invocation);
            }
            else
            {
                await invocation.ProceedAsync();
            }
        }
    }
}