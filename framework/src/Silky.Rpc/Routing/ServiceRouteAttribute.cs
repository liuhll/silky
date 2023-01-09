using System;
using Silky.Core.FilterMetadata;
using Silky.Rpc.Filters;

namespace Silky.Rpc.Routing
{
    [AttributeUsage(AttributeTargets.Interface)]
    public class ServiceRouteAttribute : Attribute, IRouteTemplateProvider, IClientFilterMetadata, IServerFilterMetadata
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