using System.Threading.Tasks;

namespace Lms.Core.DynamicProxy
{
    public interface ILmsInterceptor
    {
        Task InterceptAsync(ILmsMethodInvocation invocation);
    }
}