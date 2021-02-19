using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using Lms.Core;

namespace Lms.Rpc.Transport.CachingIntercept
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class RemoveCachingInterceptAttribute : Attribute, IRemoveCachingInterceptProvider
    {
        public RemoveCachingInterceptAttribute([NotNull]string cacheName,[NotNull]string keyTemplete)
        {
            Check.NotNullOrEmpty(cacheName, nameof(cacheName));
            Check.NotNullOrEmpty(keyTemplete, nameof(keyTemplete));
            CacheName = cacheName;
            KeyTemplete = keyTemplete;
        }
        
        public string CacheName { get; }
        public string KeyTemplete { get; }
    }
    
}