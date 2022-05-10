using Silky.Core.DynamicProxy;
using Silky.Rpc.Runtime.Server;

namespace Silky.Rpc.Extensions
{
    public static class SilkyMethodInvocationExtensions
    {
        public static ServiceEntry GetServiceEntry(this ISilkyMethodInvocation invocation)
        {
            if (invocation.ArgumentsDictionary.TryGetValue("serviceEntry", out var serviceEntry))
            {
                return serviceEntry as ServiceEntry;
            }

            return null;
        }
        
        public static ServiceEntryDescriptor GetServiceEntryDescriptor(this ISilkyMethodInvocation invocation)
        {
            if (invocation.ArgumentsDictionary.TryGetValue("serviceEntryDescriptor", out var serviceEntryDescriptor))
            {
                return serviceEntryDescriptor as ServiceEntryDescriptor;
            }

            return null;
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
        
        public static object GetServiceEntryDescriptorParameters(this ISilkyMethodInvocation invocation)
        {
            var parameters = invocation.ArgumentsDictionary["parameters"];
            return parameters;
        }
    }
}