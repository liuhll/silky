using System;
using Lms.Rpc.Address;

namespace Lms.Rpc.Runtime.Server.ServiceEntry.ServiceDiscovery
{
    [AttributeUsage(AttributeTargets.Interface)]
    public class ServiceRouteAttribute : Attribute, IRouteTemplateProvider
    {
        
        public ServiceRouteAttribute(string template = "api/{appservice}", ServiceProtocol serviceProtocol = ServiceProtocol.Tcp)
        {
            Template = template;
            ServiceProtocol = ServiceProtocol.Tcp;
        }

        public string Template { get; }
        
        public ServiceProtocol ServiceProtocol { get; }
    }
}