using System;

namespace Lms.Rpc.Runtime.Server.ServiceEntry.ServiceDiscovery
{
    [AttributeUsage(AttributeTargets.Interface)]
    public class ServiceBundleAttribute : Attribute, IServiceBundleProvider
    {
        
        public ServiceBundleAttribute(string template = "api/{appservice}")
        {
            Template = template;
            
        }

        public string Template { get; }

        public bool IsPrefix { get; }
    }
}