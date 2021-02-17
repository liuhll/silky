using System;
using Lms.Rpc.Address;

namespace Lms.Rpc.Runtime.Server.ServiceDiscovery
{
    [AttributeUsage(AttributeTargets.Interface)]
    public class ServiceRouteAttribute : Attribute, IRouteTemplateProvider
    {
        public ServiceRouteAttribute(string template = "api/{appservice}",
            ServiceProtocol serviceProtocol = ServiceProtocol.Tcp,
            bool multipleServiceKey = false)
        {
            Template = template;
            ServiceProtocol = serviceProtocol;
            MultipleServiceKey = multipleServiceKey;
        }

        public string Template { get; }

        public ServiceProtocol ServiceProtocol { get; }

        public bool MultipleServiceKey { get; }
    }
}