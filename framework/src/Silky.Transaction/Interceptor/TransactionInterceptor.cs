using System.Diagnostics;
using System.Threading.Tasks;
using Silky.Core.DependencyInjection;
using Silky.Core.DynamicProxy;
using Silky.Core.Exceptions;
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
            var serviceEntryDescriptor = invocation.GetServiceEntryDescriptor();
            if (serviceEntryDescriptor != null && serviceEntryDescriptor.IsDistributeTransaction && !RpcContext.Context.IsGateway())
            {
                throw new SilkyException(
                    "Distributed transactions must be called through the interface proxy,The station does not support the way to call through the template invoke");
            }

            var serviceEntry = invocation.GetServiceEntry();
            if (serviceEntry == null)
            {
                await invocation.ProceedAsync();
            }
            else if (!serviceEntry.IsTransactionServiceEntry())
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