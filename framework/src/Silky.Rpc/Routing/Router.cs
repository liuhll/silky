using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Silky.Core.Exceptions;
using Silky.Core.Extensions;
using Silky.Core.MethodExecutor;
using Silky.Core.Utils;
using Silky.Rpc.Runtime.Server.Parameter;
using Microsoft.AspNetCore.Server.Kestrel.Core.Internal.Http;
using Silky.Rpc.Routing.Template;

namespace Silky.Rpc.Routing
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
            if (apiSegments.Length != RouteTemplate.Segments.Count())
            {
                return false;
            }

            var parameterIndex = 0;
            var apiSegmentIsMatch = new List<bool>(apiSegments.Length);
            for (var index = 0; index < apiSegments.Length; index++)
            {
                var apiSegment = apiSegments[index];
                var routeSegment = RouteTemplate.Segments[index];
                if (!routeSegment.IsParameter &&
                    !routeSegment.Value.Equals(apiSegment, StringComparison.OrdinalIgnoreCase))
                {
                    apiSegmentIsMatch.Add(false);
                    break;
                }
                if (routeSegment.IsParameter)
                {
                    if (parameterIndex >= RouteTemplate.Parameters.Count())
                    {
                        apiSegmentIsMatch.Add(false);
                        break;
                    }

                    var routeParameter = RouteTemplate.Parameters[parameterIndex];
                    if (!routeParameter.Constraint.IsNullOrEmpty())
                    {
                        if (TypeUtils.GetSampleType(routeParameter.Constraint, out var convertType))
                        {
                            try
                            {
                                var val = Convert.ChangeType(apiSegment, convertType);
                                apiSegmentIsMatch.Add(true);
                                continue;
                            }
                            catch (Exception e)
                            {
                                apiSegmentIsMatch.Add(false);
                                break;
                            }
                        }

                        if (!Regex.IsMatch(apiSegment, routeParameter.Constraint))
                        {
                            apiSegmentIsMatch.Add(false);
                        }
                    }
                }

                apiSegmentIsMatch.Add(true);
            }

            return apiSegmentIsMatch.All(p => p);
        }

        public IDictionary<string, object> ParserRouteParameters(string path)
        {
            var routeParameters = new Dictionary<string, object>();
            var api = TrimPrefix(path);
            var apiSegments = api.Split(separator);
            for (var index = 0; index < apiSegments.Length; index++)
            {
                var apiSegment = apiSegments[index];
                var routeSegment = RouteTemplate.Segments[index];
                if (!routeSegment.IsParameter)
                {
                    continue;
                }

                routeParameters.Add(TemplateSegmentHelper.GetVariableName(routeSegment.Value), apiSegment);
            }

            return routeParameters;
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
                throw new SilkyException("The routing template does not specify a service application",
                    StatusCode.RouteParseError);
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
                throw new SilkyException(
                    "The setting of routing parameters is abnormal, and it is only allowed to set path parameters for simple data types",
                    StatusCode.RouteParseError);
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
                throw new SilkyException($"{template} The format of the route template set is abnormal",
                    StatusCode.RouteParseError);
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