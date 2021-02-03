using System;

namespace Lms.Rpc.Runtime.Server.ServiceEntry.ServiceDiscovery
{
    [AttributeUsage(AttributeTargets.Interface)]
    public class ServiceBundleAttribute : Attribute, IServiceBundleProvider
    {
        
        public ServiceBundleAttribute(string template = "api/{appservice}", ServiceProtocol serviceProtocol = ServiceProtocol.Tcp)
        {
            Template = template;
            ServiceProtocol = ServiceProtocol.Tcp;
        }

        public string Template { get; }
        
        public ServiceProtocol ServiceProtocol { get; }
    }
}