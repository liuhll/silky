using System.Diagnostics;
using System.Threading.Tasks;
using Silky.Core.DependencyInjection;
using Silky.Core.DynamicProxy;
using Silky.Core.Runtime.Rpc;
using Silky.Rpc.Extensions;
using Silky.Rpc.Runtime.Server;
using Silky.Transaction.Abstraction;

namespace Silky.Transaction.Interceptor
{
    public class TransactionInterceptor : SilkyInterceptor, ITransientDependency
    {
        public override async Task InterceptAsync(ISilkyMethodInvocation invocation)
        {
            var serviceEntry = invocation.GetServiceEntry();
            Debug.Assert(serviceEntry != null);
            if (!serviceEntry.IsTransactionServiceEntry())
            {
                await invocation.ProceedAsync();
            }
            else
            {
                var transactionContext = RpcContext.Context.GetTransactionContext();
                await TransactionAspectInvoker.GetInstance().Invoke(transactionContext, invocation);
            }
        }
    }
}