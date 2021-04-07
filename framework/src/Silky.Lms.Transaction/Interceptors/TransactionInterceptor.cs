using System.Diagnostics;
using System.Threading.Tasks;
using Silky.Lms.Core.DependencyInjection;
using Silky.Lms.Core.DynamicProxy;
using Silky.Lms.Rpc.Runtime.Server;
using Silky.Lms.Rpc.Transport;

namespace Silky.Lms.Transaction.Interceptors
{
    public class TransactionInterceptor : LmsInterceptor, ITransientDependency
    {
        public async override Task InterceptAsync(ILmsMethodInvocation invocation)
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