using System;
using JetBrains.Annotations;
using Lms.Core;

namespace Lms.Rpc.Transport.CachingIntercept
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class UpdateCachingInterceptAttribute : Attribute, IUpdateCachingInterceptProvider
    {
        public UpdateCachingInterceptAttribute([NotNull]string cacheName,[NotNull]string keyTemplete)
        {
            Check.NotNullOrEmpty(cacheName, nameof(cacheName));
            Check.NotNullOrEmpty(keyTemplete, nameof(keyTemplete));
            CacheName = cacheName;
            KeyTemplete = keyTemplete;
            OnlyCurrentUserData = false;
        }
        
        public string KeyTemplete { get; }
        
        public bool OnlyCurrentUserData { get; set; }
        public string CacheName { get; }
    }
}