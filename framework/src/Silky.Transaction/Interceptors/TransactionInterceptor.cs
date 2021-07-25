using System.Diagnostics;
using System.Threading.Tasks;
using Silky.Core.DependencyInjection;
using Silky.Core.DynamicProxy;
using Silky.Rpc.Runtime.Server;
using Silky.Rpc.Transport;
using Silky.Transaction.Abstraction;

namespace Silky.Transaction.Interceptors
{
    public class TransactionInterceptor : SilkyInterceptor, ITransientDependency
    {
        public async override Task InterceptAsync(ISilkyMethodInvocation invocation)
        {
            var argumentsDictionary = invocation.ArgumentsDictionary;
            var serviceEntry = argumentsDictionary["serviceEntry"] as ServiceEntry;
            Debug.Assert(serviceEntry != null);
            if (!serviceEntry.IsTransactionServiceEntry())
            {
                await invocation.ProceedAsync();
            }
            else
            {
                var transactionContext = RpcContext.GetContext().GetTransactionContext();
                await TransactionAspectInvoker.GetInstance().Invoke(transactionContext, invocation);
            }
        }
    }
}