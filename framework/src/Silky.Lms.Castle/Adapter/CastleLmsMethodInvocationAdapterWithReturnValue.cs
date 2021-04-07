using System;
using System.Threading.Tasks;
using Castle.DynamicProxy;
using Silky.Lms.Core.DynamicProxy;

namespace Silky.Lms.Castle.Adapter
{
    public class CastleLmsMethodInvocationAdapterWithReturnValue<TResult> : CastleLmsMethodInvocationAdapterBase, ILmsMethodInvocation
    {
        protected IInvocationProceedInfo ProceedInfo { get; }
        protected Func<IInvocation, IInvocationProceedInfo, Task<TResult>> Proceed { get; }

        public CastleLmsMethodInvocationAdapterWithReturnValue(IInvocation invocation,
            IInvocationProceedInfo proceedInfo,
            Func<IInvocation, IInvocationProceedInfo, Task<TResult>> proceed)
            : base(invocation)
        {
            ProceedInfo = proceedInfo;
            Proceed = proceed;
        }

        public override async Task ProceedAsync()
        {
            ReturnValue = await Proceed(Invocation, ProceedInfo);
        }
    }
}
