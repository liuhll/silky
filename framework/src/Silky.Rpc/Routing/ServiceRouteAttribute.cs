using System;

namespace Silky.Rpc.Routing
{
    [AttributeUsage(AttributeTargets.Interface)]
    public class ServiceRouteAttribute : Attribute, IRouteTemplateProvider
    {
        public ServiceRouteAttribute()
            : this("api/{appservice}")
        {
        }

        public ServiceRouteAttribute(string template)
        {
            Template = template;
        }

        public string Template { get; }

        public string ServiceName { get; set; }
    }
}