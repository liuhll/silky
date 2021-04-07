using System;
using Silky.Lms.Core;
using Silky.Lms.Rpc.Configuration;
using Microsoft.Extensions.Options;
using Silky.Lms.Rpc.Address;

namespace Silky.Lms.Rpc.Runtime.Server.ServiceDiscovery
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
    }
}