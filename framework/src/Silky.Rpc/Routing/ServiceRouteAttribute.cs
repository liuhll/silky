using System;
using Silky.Core;

namespace Silky.Rpc.Routing
{
    [AttributeUsage(AttributeTargets.Interface)]
    public class ServiceRouteAttribute : Attribute, IRouteTemplateProvider
    {
        public ServiceRouteAttribute(string template = "api/{appservice}")
        {
            Template = template;
        }

        public string Template { get; }

        public string ServiceName { get; set; }
    }
}