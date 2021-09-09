using System;
using JetBrains.Annotations;
using Silky.Core;

namespace Silky.Rpc.Transport.CachingIntercept
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class UpdateCachingInterceptAttribute : Attribute, IUpdateCachingInterceptProvider
    {
        public UpdateCachingInterceptAttribute([NotNull]string keyTemplete)
        {
            Check.NotNullOrEmpty(keyTemplete, nameof(keyTemplete));
            KeyTemplete = keyTemplete;
            OnlyCurrentUserData = false;
            CachingMethod = CachingMethod.Remove;
        }
        
        public string KeyTemplete { get; }
        
        public bool OnlyCurrentUserData { get; set; }
        
        public CachingMethod CachingMethod { get; }
    }
}