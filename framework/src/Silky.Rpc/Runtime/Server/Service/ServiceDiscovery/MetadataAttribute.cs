using System;
using JetBrains.Annotations;
using Silky.Core;

namespace Silky.Rpc.Runtime.Server.ServiceDiscovery
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Interface, AllowMultiple = true)]
    public class MetadataAttribute : Attribute
    {
        public string Key { get; private set; }

        public string Value { get; private set; }

        public MetadataAttribute([NotNull] string key, [NotNull] string value)
        {
            Check.NotNull(key, nameof(key));
            Check.NotNull(value, nameof(value));
            Key = key;
            Value = value;
        }
    }
}