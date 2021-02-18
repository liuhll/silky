using System;

namespace Lms.Rpc.Transport.CachingIntercept
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class GetCachingInterceptAttribute : Attribute, IGetCachingInterceptProvider
    {
        public GetCachingInterceptAttribute(string cacheName)
        {
            CacheName = cacheName;
            CachingMethod = CachingMethod.Get;
        }

        public string CacheName { get; }

        public CachingMethod CachingMethod { get; }
    }
}