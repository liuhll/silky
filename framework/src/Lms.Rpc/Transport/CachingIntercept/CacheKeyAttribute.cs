using System;

namespace Lms.Rpc.Transport.CachingIntercept
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter, AllowMultiple = false)]
    public class CacheKeyAttribute : Attribute, ICacheKeyProvider
    {
        private string _propName;

        public CacheKeyAttribute(int order)
        {
            Order = order;
        }

        public int Order { get; }

        public string Name { get; set; }

        string ICacheKeyProvider.PropName
        {
            get => _propName;
            set => _propName = value;
        }
    }
}