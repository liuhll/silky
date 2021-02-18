using System;

namespace Lms.Rpc.Transport.CachingIntercept
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class RemoveCachingInterceptAttribute : Attribute, IRemoveCachingInterceptProvider
    {
        public RemoveCachingInterceptAttribute(string[] cacheName, string keyTemplet)
        {
            CacheName = cacheName;
            CachingMethod = CachingMethod.Remove;
        }

        public string[] CacheName { get; }

        public CachingMethod CachingMethod { get; }
        
    }
}