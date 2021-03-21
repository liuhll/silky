using System;
using System.Linq;
using System.Reflection;
using Lms.Core.Exceptions;
using Lms.Rpc.Address;
using Lms.Rpc.Routing.Template;

namespace Lms.Rpc.Utils
{
    public static class WebSocketResolverHelper
    {
        private const string separator = "/";

        public static string ParseWsPath(Type wsAppServiceType)
        {
            var routeTemplateProvider = wsAppServiceType.GetCustomAttributes().OfType<IRouteTemplateProvider>()
                .FirstOrDefault();
            if (routeTemplateProvider == null)
            {
                throw new LmsException("ws服务必须要通过WsServiceRoute特性进行注解");
            }

            return ParseWsPath(routeTemplateProvider.Template, wsAppServiceType.Name);
        }

        public static string Generator(string wsPath)
        {
            wsPath = wsPath.TrimStart('/');
            return wsPath.Replace("/", "_");
        }

        public static int GetWsRpcPort(Type wsAppServiceType)
        {
            var routeTemplateProvider = wsAppServiceType.GetCustomAttributes().OfType<IRouteTemplateProvider>()
                .FirstOrDefault();
            if (routeTemplateProvider == null)
            {
                throw new LmsException("ws服务必须要通过WsServiceRoute特性进行注解");
            }

            return routeTemplateProvider.RpcPort;
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
                throw new LmsException($"{template}设置的路由模板格式不正常", StatusCode.RouteParseError);
            }

            return template;
        }

        public static string ParseWsPath(string template, string serviceName)
        {
            var wsPath = string.Empty;
            template = TrimPrefix(template);
            var segementLines = template.Split(separator);
            foreach (var segementLine in segementLines)
            {
                var segmentType = TemplateSegmentHelper.GetSegmentType(segementLine, serviceName);
                switch (segmentType)
                {
                    case SegmentType.Literal:
                        wsPath += "/" + segementLine;
                        break;
                    case SegmentType.AppService:
                    {
                        var appServiceName = ParseAppServiceName(TemplateSegmentHelper.GetSegmentVal(segementLine),
                            serviceName);
                        wsPath += "/" + appServiceName;
                        break;
                    }
                    default:
                        throw new LmsException("解析websocketPath失败");
                }
            }

            return wsPath.TrimEnd('/');
        }

        private static string ParseAppServiceName(string segemnetVal, string serviceName)
        {
            if (segemnetVal.Contains("="))
            {
                return segemnetVal.Split("=")[1].ToLower();
            }

            if (serviceName.StartsWith("I"))
            {
                return serviceName.Substring(1, serviceName.Length - segemnetVal.Length - 1).ToLower();
            }

            return serviceName.Substring(0, serviceName.Length - segemnetVal.Length).ToLower();
        }
    }
}