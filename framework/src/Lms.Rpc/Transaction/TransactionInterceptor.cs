using System.Threading.Tasks;
using Lms.Core.DependencyInjection;
using Lms.Core.DynamicProxy;

namespace Lms.Rpc.Transaction
{
    public class TransactionInterceptor : LmsInterceptor, ITransientDependency
    {
        public TransactionInterceptor()
        {
            
        }

        public async override Task InterceptAsync(ILmsMethodInvocation invocation)
        {
            await invocation.ProceedAsync();
        }
    }
}