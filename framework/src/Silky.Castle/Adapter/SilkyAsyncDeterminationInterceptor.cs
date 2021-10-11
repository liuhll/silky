using Castle.DynamicProxy;
using Silky.Core.DynamicProxy;

namespace Silky.Castle.Adapter
{
    public class SilkyAsyncDeterminationInterceptor<TInterceptor> : AsyncDeterminationInterceptor
        where TInterceptor : ISilkyInterceptor
    {
        public SilkyAsyncDeterminationInterceptor(TInterceptor abpInterceptor)
            : base(new CastleAsyncSilkyInterceptorAdapter<TInterceptor>(abpInterceptor))
        {
        }
    }
}