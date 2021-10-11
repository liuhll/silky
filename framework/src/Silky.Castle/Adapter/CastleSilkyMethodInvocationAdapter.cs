using System;
using System.Threading.Tasks;
using Castle.DynamicProxy;
using Silky.Core.DynamicProxy;

namespace Silky.Castle.Adapter
{
    public class CastleSilkyMethodInvocationAdapter : CastleSilkyMethodInvocationAdapterBase, ISilkyMethodInvocation
    {
        protected IInvocationProceedInfo ProceedInfo { get; }
        protected Func<IInvocation, IInvocationProceedInfo, Task> Proceed { get; }

        public CastleSilkyMethodInvocationAdapter(IInvocation invocation, IInvocationProceedInfo proceedInfo,
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