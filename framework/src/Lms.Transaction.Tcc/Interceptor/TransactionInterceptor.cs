using System.Diagnostics;
using System.Threading.Tasks;
using Lms.Core.DependencyInjection;
using Lms.Core.DynamicProxy;
using Lms.Rpc.Runtime.Server;

namespace Lms.Transaction.Tcc.Interceptor
{
    public class TransactionInterceptor : LmsInterceptor, ITransientDependency
    {
        public TransactionInterceptor()
        {
            
        }

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
                if (serviceEntry.IsLocal)
                {
                    var serviceKey = argumentsDictionary["serviceKey"] as string;
                    var tccTransactionProvider = serviceEntry.GetTccTransactionProvider(serviceKey);
                }
                
            }

        }
    }
}