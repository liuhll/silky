using System.Threading.Tasks;

namespace Lms.Core.DynamicProxy
{
    public abstract class LmsInterceptor : ILmsInterceptor
    {
        public abstract Task InterceptAsync(ILmsMethodInvocation invocation);
    }
}