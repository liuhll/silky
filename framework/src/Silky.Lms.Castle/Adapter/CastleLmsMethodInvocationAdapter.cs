using System;
using System.Threading.Tasks;
using Castle.DynamicProxy;
using Silky.Lms.Core.DynamicProxy;

namespace Silky.Lms.Castle.Adapter
{
    public class CastleLmsMethodInvocationAdapter : CastleLmsMethodInvocationAdapterBase, ILmsMethodInvocation
    {
        protected IInvocationProceedInfo ProceedInfo { get; }
        protected Func<IInvocation, IInvocationProceedInfo, Task> Proceed { get; }

        public CastleLmsMethodInvocationAdapter(IInvocation invocation, IInvocationProceedInfo proceedInfo,
            Func<IInvocation, IInvocationProceedInfo, Task> proceed)
            : base(invocation)
        {
            ProceedInfo = proceedInfo;
            Proceed = proceed;
        }

        public override async Task ProceedAsync()
        {
            await Proceed(Invocation, ProceedInfo);
        }
    }
}
