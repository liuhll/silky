using System;

namespace Lms.Rpc.Address
{
    [AttributeUsage(AttributeTargets.Method)]
    public class ServiceRouteAttribute : Attribute, IServiceRouteProvider
    {
        public ServiceRouteAttribute(string template)
        {
            Template = template;
        }
         
        public string Template { get; }  
    }
}