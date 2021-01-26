using System;

namespace Lms.Rpc.Runtime.Server.ServiceEntry.ServiceDiscovery
{
    [AttributeUsage(AttributeTargets.Interface)]
    public class ServiceBundleAttribute : Attribute, IServiceBundleProvider
    {
        
        public ServiceBundleAttribute(string template = "{appservice=appservice}/{entry=method}", bool isPrefix = true)
        {
            Template = template;
            IsPrefix = isPrefix;
        }

        public string Template { get; }

        public bool IsPrefix { get; }
    }
}