using System;

namespace Silky.Rpc.Runtime.Server
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Interface)]
    public class FallbackAttribute : Attribute, IFallbackProvider
    {
        public FallbackAttribute(Type type)
        {
            Type = type;
        }

        public Type Type { get; }
        public string MethodName { get; set; }
      
    }
}