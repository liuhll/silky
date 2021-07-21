using System;
using JetBrains.Annotations;
using Silky.Core;
using Silky.Core.Exceptions;
using Silky.Rpc.Runtime.Server;
using Silky.Rpc.Routing.Template;

namespace Silky.Rpc.Address
{
    public interface IRouteTemplateProvider
    {
        string Template { get; }

        bool MultipleServiceKey { get; }
    }

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
                throw new SilkyException($"{template}设置的路由模板格式不正常", StatusCode.RouteParseError);
            }

            return template;
        }
    }
}