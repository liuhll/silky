using System;
using System.Threading.Tasks;
using Castle.DynamicProxy;
using Silky.Core.DynamicProxy;

namespace Silky.Castle.Adapter
{
    public class CastleAsyncSilkyInterceptorAdapter<TInterceptor> : AsyncInterceptorBase
        where TInterceptor : ISilkyInterceptor
    {
        private readonly TInterceptor _silkyInterceptor;

        public CastleAsyncSilkyInterceptorAdapter(TInterceptor silkyInterceptor)
        {
            _silkyInterceptor = silkyInterceptor;
        }

        protected override async Task InterceptAsync(IInvocation invocation, IInvocationProceedInfo proceedInfo,
            Func<IInvocation, IInvocationProceedInfo, Task> proceed)
        {
            await _silkyInterceptor.InterceptAsync(
                new CastleSilkyMethodInvocationAdapter(invocation, proceedInfo, proceed)
            );
        }

        protected override async Task<TResult> InterceptAsync<TResult>(IInvocation invocation,
            IInvocationProceedInfo proceedInfo, Func<IInvocation, IInvocationProceedInfo, Task<TResult>> proceed)
        {
            var adapter =
                new CastleSilkyMethodInvocationAdapterWithReturnValue<TResult>(invocation, proceedInfo, proceed);

            await _silkyInterceptor.InterceptAsync(
                adapter
            );

            return (TResult)adapter.ReturnValue;
        }
    }
}