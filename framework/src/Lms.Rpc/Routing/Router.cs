using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Lms.Core.Exceptions;
using Lms.Core.Extensions;
using Lms.Rpc.Routing.Template;
using Lms.Rpc.Runtime.Server.ServiceEntry.Parameter;
using Microsoft.AspNetCore.Server.Kestrel.Core.Internal.Http;

namespace Lms.Rpc.Routing
{
    public class Router : IRouter
    {
        private const string separator = "/";
        private readonly MethodInfo _method;
        private readonly string _routePath;

        public Router(string template, string serviceName, MethodInfo methodInfo, HttpMethod httpMethod)
        {
            RouteTemplate = new RouteTemplate();
            ParseRouteTemplate(template, serviceName, methodInfo);
            HttpMethod = httpMethod;
            _method = methodInfo;
            _routePath = GenerateRoutePath();
        }

        private string GenerateRoutePath()
        {
            var routePath = string.Empty;
            foreach (var segment in RouteTemplate.Segments)
            {
                if (!segment.IsParameter)
                {
                    routePath += segment.Value + "/";
                }
                else
                {
                    routePath += "{" + TemplateSegmentHelper.GetSegmentVal(segment.Value) + "}" + "/";
                }
            }

            return routePath.ToLower().TrimEnd('/');
        }

        public RouteTemplate RouteTemplate { get; }

        public HttpMethod HttpMethod { get; }

        public string RoutePath => _routePath;

        public bool IsMatch(string api, HttpMethod httpMethod)
        {
            if (HttpMethod != httpMethod)
            {
                return false;
            }

            api = TrimPrefix(api);
            var apiSegments = api.Split(separator);
            var parameterIndex = 0;
            for (var index = 0; index < apiSegments.Length; index++)
            {
                var apiSegment = apiSegments[index];
                var routeSegment = RouteTemplate.Segments[index];
                if (!routeSegment.IsParameter && !routeSegment.Value.Equals(apiSegment))
                {
                    return false;
                }

                if (routeSegment.IsParameter)
                {
                    if (parameterIndex >= RouteTemplate.Parameters.Count())
                    {
                        return false;
                    }

                    var routeParameter = RouteTemplate.Parameters[parameterIndex];
                    if (!routeParameter.Constraint.IsNullOrEmpty() &&
                        !Regex.IsMatch(apiSegment, routeParameter.Constraint))
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        private void ParseRouteTemplate(string template, string serviceName, MethodInfo methodInfo)
        {
            template = TrimPrefix(template);
            var segementLines = template.Split(separator);

            foreach (var segementLine in segementLines)
            {
                var segmentType = TemplateSegmentHelper.GetSegmentType(segementLine, serviceName);
                switch (segmentType)
                {
                    case SegmentType.Literal:
                        RouteTemplate.Segments.Add(new TemplateSegment(SegmentType.Literal, segementLine));
                        break;
                    case SegmentType.AppService:
                    {
                        var appServiceName = ParseAppServiceName(TemplateSegmentHelper.GetSegmentVal(segementLine),
                            serviceName);
                        RouteTemplate.Segments.Add(new TemplateSegment(SegmentType.AppService, appServiceName));
                        break;
                    }
                    case SegmentType.Path:
                    {
                        var pathParameterSegment =
                            ParsePathParameterSegment(TemplateSegmentHelper.GetSegmentVal(segementLine), methodInfo);
                        RouteTemplate.Segments.Add(pathParameterSegment.Item1);
                        RouteTemplate.Parameters.Add(pathParameterSegment.Item2);
                        break;
                    }
                }
            }

            var appServiceSegmentCount = RouteTemplate.Segments?.Count(p => p.SegmentType == SegmentType.AppService);
            if (appServiceSegmentCount != 1)
            {
                throw new LmsException("路由模板未指定服务应用", StatusCode.RouteParseError);
            }
        }

        private (TemplateSegment, TemplateParameter) ParsePathParameterSegment(string segemnetVal,
            MethodInfo methodInfo)
        {
            var parameterName = segemnetVal;
            string constraint = null;
            if (segemnetVal.Contains(":"))
            {
                parameterName = segemnetVal.Split(":")[0];
                constraint = segemnetVal.Split(":")[1];
            }

            if (!methodInfo.GetParameters().Any(p =>
                p.Name.Equals(parameterName, StringComparison.OrdinalIgnoreCase) && p.IsSampleType()))
            {
                throw new LmsException("设置路由参数不正常,只允许为简单数据类型设置路径参数", StatusCode.RouteParseError);
            }

            return (new TemplateSegment(SegmentType.Path, segemnetVal),
                new TemplateParameter(parameterName, constraint));
        }

        private string ParseAppServiceName(string segemnetVal, string serviceName)
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
                throw new LmsException($"{template}设置的路由模板格式不正常", StatusCode.RouteParseError);
            }

            return template;
        }

        public override bool Equals(object obj)
        {
            if (obj is null)
            {
                return false;
            }

            if (!(obj is IRouter router))
            {
                return false;
            }

            return HttpMethod == router.HttpMethod && RoutePath.Equals(router.RoutePath);
        }

        public override int GetHashCode()
        {
            return (RoutePath + HttpMethod).GetHashCode();
        }

        public static bool operator ==(Router model1, Router model2)
        {
            return Equals(model1, model2);
        }

        public static bool operator !=(Router model1, Router model2)
        {
            return !Equals(model1, model2);
        }
    }
}