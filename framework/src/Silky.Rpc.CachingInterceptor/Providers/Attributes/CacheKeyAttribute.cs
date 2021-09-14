using System;
using Silky.Rpc.Runtime.Server;

namespace Silky.Rpc.CachingInterceptor
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter, AllowMultiple = false)]
    public class CacheKeyAttribute : Attribute, ICacheKeyProvider
    {
        public CacheKeyAttribute(int index)
        {
            Index = index;
        }

        public int Index { get; }
        public string PropName { get; set; }
        public string Value { get; set; }
    }
}