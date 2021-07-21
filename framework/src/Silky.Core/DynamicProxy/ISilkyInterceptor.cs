using System.Threading.Tasks;

namespace Silky.Core.DynamicProxy
{
    public interface ISilkyInterceptor
    {
        Task InterceptAsync(ISilkyMethodInvocation invocation);
    }
}