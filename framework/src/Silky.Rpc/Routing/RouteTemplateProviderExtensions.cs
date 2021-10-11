using System;
using Castle.Core.Internal;
using JetBrains.Annotations;
using Silky.Core;
using Silky.Core.Exceptions;
using Silky.Rpc.Routing.Template;

namespace Silky.Rpc.Routing
{
    public static class RouteTemplateProviderExtensions
    {
        private const string separator = "/";

        public static string GetWsPath([NotNull] this IRouteTemplateProvider routeTemplateProvider, string serviceName)
        {
            Check.NotNull(routeTemplateProvider, nameof(routeTemplateProvider));
            var template = TrimPrefix(routeTemplateProvider.Template);
            var segementLines = template.Split(separator);
            var wsPath = "/";
            foreach (var segementLine in segementLines)
            {
                var segmentType = TemplateSegmentHelper.GetSegmentType(segementLine, serviceName);
                if (segmentType == SegmentType.AppService)
                {
                    var appServiceName = ParseAppServiceName(TemplateSegmentHelper.GetSegmentVal(segementLine),
                        serviceName);
                    wsPath += appServiceName;
                }
                else
                {
                    wsPath += TemplateSegmentHelper.GetSegmentVal(segementLine);
                }
            }

            return wsPath;
        }

        public static string GetServiceName([NotNull] this IRouteTemplateProvider routeTemplateProvider,
            Type serviceType)
        {
            return routeTemplateProvider.ServiceName.IsNullOrEmpty()
                ? serviceType.Name.TrimStart('I')
                : routeTemplateProvider.ServiceName;
        }

        private static string ParseAppServiceName(string segemnetVal, string serviceName)
        {
            if (segemnetVal.Contains("="))
            {
                return segemnetVal.Split("=")[1].ToLower();
            }

            return serviceName.Substring(1, serviceName.Length - segemnetVal.Length - 1).ToLower();
        }

        private static string TrimPrefix(string template)
        {
            if (template.StartsWith("~/", StringComparison.Ordinal))
            {
                return template.Substring(2);
            }
            else if (template.StartsWith("/", StringComparison.Ordinal))
            {
                return template.Substring(1);
            }
            else if (template.StartsWith("~", StringComparison.Ordinal))
            {
                throw new SilkyException($"{template} The format of the route template set is incorrect",
                    StatusCode.RouteParseError);
            }

            return template;
        }
    }
}