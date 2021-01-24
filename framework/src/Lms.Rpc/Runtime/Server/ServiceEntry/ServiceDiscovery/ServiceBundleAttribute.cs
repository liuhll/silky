using System;

namespace Lms.Rpc.Runtime.Server.ServiceEntry.ServiceDiscovery
{
    [AttributeUsage(AttributeTargets.Interface)]
    public class ServiceBundleAttribute : Attribute, IServiceBundleProvider
    {
        public ServiceBundleAttribute(string routeTemplate, bool isPrefix = true)
        {
            RouteTemplate = routeTemplate;
            IsPrefix = isPrefix;
        }

        public string RouteTemplate { get; }

        public bool IsPrefix { get; }
    }
}