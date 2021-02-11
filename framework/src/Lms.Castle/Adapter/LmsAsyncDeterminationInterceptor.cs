using Castle.DynamicProxy;
using Lms.Core.DynamicProxy;

namespace Lms.Castle.Adapter
{
    public class LmsAsyncDeterminationInterceptor<TInterceptor> : AsyncDeterminationInterceptor
        where TInterceptor : ILmsInterceptor
    {
        public LmsAsyncDeterminationInterceptor(TInterceptor abpInterceptor)
            : base(new CastleAsyncLmsInterceptorAdapter<TInterceptor>(abpInterceptor))
        {

        }
    }
}