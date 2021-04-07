using System.Threading.Tasks;

namespace Silky.Lms.Core.DynamicProxy
{
    public interface ILmsInterceptor
    {
        Task InterceptAsync(ILmsMethodInvocation invocation);
    }
}