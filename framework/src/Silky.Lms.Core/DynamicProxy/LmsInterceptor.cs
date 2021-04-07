using System.Threading.Tasks;

namespace Silky.Lms.Core.DynamicProxy
{
    public abstract class LmsInterceptor : ILmsInterceptor
    {
        public abstract Task InterceptAsync(ILmsMethodInvocation invocation);
    }
}