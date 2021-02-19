using System;

namespace Lms.Rpc.Transport.CachingIntercept
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter, AllowMultiple = false)]
    public class CacheKeyAttribute : Attribute, ICacheKeyProvider
    {
        private string _propName;
        private string _value;

        public CacheKeyAttribute(int index)
        {
            Index = index;
        }

        public int Index { get; }

        string ICacheKeyProvider.PropName
        {
            get => _propName;
            set => _propName = value;
        }

        string ICacheKeyProvider.Value
        {
            get => _value;
            set => _value = value;
        }
    }
}