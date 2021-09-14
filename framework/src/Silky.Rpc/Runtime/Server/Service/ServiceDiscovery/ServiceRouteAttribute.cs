using System;
using Silky.Core;
using Silky.Rpc.Routing;

namespace Silky.Rpc.Runtime.Server
{
    [AttributeUsage(AttributeTargets.Interface)]
    public class ServiceRouteAttribute : Attribute, IRouteTemplateProvider
    {
        public ServiceRouteAttribute(string template = "api/{appservice}",
            bool multipleServiceKey = false)
        {
            Template = template;
            MultipleServiceKey = multipleServiceKey;
            Application = EngineContext.Current.HostName;
        }

        public string Template { get; }

        public bool MultipleServiceKey { get; }

        public string ServiceName { get; set; }
        
        public string Application { get; set; }
    }
}