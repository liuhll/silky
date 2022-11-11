using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Silky.Core.Exceptions;
using Silky.Rpc.Routing.Template;

namespace Silky.Rpc.Routing;

public static class RoutePathHelper
{
    private const string separator = "/";

    public static IDictionary<string, object> ParserRouteParameters(string routeTemplate, string path)
    {
        var routeParameters = new Dictionary<string, object>();
        var api = TrimPrefix(path);
        routeTemplate = TrimPrefix(routeTemplate);
        var routeTemplateSegments = routeTemplate.Split(separator);
        var apiSegments = api.Split(separator);

        var index = 0;
        foreach (var routeTemplateSegment in routeTemplateSegments)
        {
            if (Regex.IsMatch(routeTemplateSegment, RouterConstants.PathRegex))
            {
                var pathParamName = routeTemplateSegment.Replace("{", "").Replace("}", "").Split(":")[0];
                routeParameters.Add(pathParamName, apiSegments[index]);
            }

            index++;
        }

        return routeParameters;
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
}