using System;
using Silky.Rpc.Address;

namespace Silky.Rpc.Runtime.Server.ServiceDiscovery
{
    [AttributeUsage(AttributeTargets.Interface)]
    public class ServiceRouteAttribute : Attribute, IRouteTemplateProvider
    {
        public ServiceRouteAttribute(string template = "api/{appservice}",
            bool multipleServiceKey = false)
        {
            Template = template;
            MultipleServiceKey = multipleServiceKey;
        }

        public string Template { get; }

        public bool MultipleServiceKey { get; }

        public string ServiceName { get; set; }
    }
}