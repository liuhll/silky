using Silky.Core.DynamicProxy;
using Silky.Rpc.Runtime.Server;

namespace Silky.Rpc.Extensions
{
    public static class SilkyMethodInvocationExtensions
    {
        public static ServiceEntry GetServiceEntry(this ISilkyMethodInvocation invocation)
        {
            var serviceEntry = invocation.ArgumentsDictionary["serviceEntry"] as ServiceEntry;
            return serviceEntry;
        }

        public static string GetServiceKey(this ISilkyMethodInvocation invocation)
        {
            var serviceKey = invocation.ArgumentsDictionary["serviceKey"] as string;
            return serviceKey;
        }

        public static object[] GetParameters(this ISilkyMethodInvocation invocation)
        {
            var parameters = invocation.ArgumentsDictionary["parameters"] as object[];
            return parameters;
        }
    }
}