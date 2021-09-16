using System;

namespace Silky.Rpc.Runtime.Server
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Interface)]
    public class FallbackAttribute : Attribute, IFallbackProvider
    {
        public FallbackAttribute(Type type)
        {
            Type = type;
            ValidWhenBusinessException = true;
        }

        public Type Type { get; }

        public string MethodName { get; set; }
        public int Weight { get; set; }
        
        public string ServiceName { get; set; }

        public bool ValidWhenBusinessException { get; set; }
    }
}