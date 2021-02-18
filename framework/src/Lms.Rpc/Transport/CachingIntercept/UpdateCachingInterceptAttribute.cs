using System;

namespace Lms.Rpc.Transport.CachingIntercept
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class UpdateCachingInterceptAttribute : Attribute, IUpdateCachingInterceptProvider
    {
        public UpdateCachingInterceptAttribute(string[] cacheName)
        {
            CacheName = cacheName;
            CachingMethod = CachingMethod.Update;
        }

        public CachingMethod CachingMethod { get; }
        public string[] CacheName { get; }
    }
}