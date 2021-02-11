using System;
using System.Threading.Tasks;
using Castle.DynamicProxy;
using Lms.Core.DynamicProxy;

namespace Lms.Castle.Adapter
{
    public class CastleAsyncLmsInterceptorAdapter<TInterceptor> : AsyncInterceptorBase
        where TInterceptor : ILmsInterceptor
    {
        private readonly TInterceptor _lmsInterceptor;

        public CastleAsyncLmsInterceptorAdapter(TInterceptor lmsInterceptor)
        {
            _lmsInterceptor = lmsInterceptor;
        }

        protected override async Task InterceptAsync(IInvocation invocation, IInvocationProceedInfo proceedInfo, Func<IInvocation, IInvocationProceedInfo, Task> proceed)
        {
            await _lmsInterceptor.InterceptAsync(
                new CastleLmsMethodInvocationAdapter(invocation, proceedInfo, proceed)
            );
        }

        protected override async Task<TResult> InterceptAsync<TResult>(IInvocation invocation, IInvocationProceedInfo proceedInfo, Func<IInvocation, IInvocationProceedInfo, Task<TResult>> proceed)
        {
            var adapter = new CastleLmsMethodInvocationAdapterWithReturnValue<TResult>(invocation, proceedInfo, proceed);

            await _lmsInterceptor.InterceptAsync(
                adapter
            );

            return (TResult)adapter.ReturnValue;
        }
    }
}