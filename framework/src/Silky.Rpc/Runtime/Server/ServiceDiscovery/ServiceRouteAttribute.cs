using System;
using Silky.Core;
using Silky.Rpc.Configuration;
using Microsoft.Extensions.Options;
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
    }
}