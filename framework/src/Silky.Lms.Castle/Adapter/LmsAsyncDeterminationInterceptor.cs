using Castle.DynamicProxy;
using Silky.Lms.Core.DynamicProxy;

namespace Silky.Lms.Castle.Adapter
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