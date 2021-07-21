using System.Threading.Tasks;

namespace Silky.Core.DynamicProxy
{
    public abstract class SilkyInterceptor : ISilkyInterceptor
    {
        public abstract Task InterceptAsync(ISilkyMethodInvocation invocation);
    }
}