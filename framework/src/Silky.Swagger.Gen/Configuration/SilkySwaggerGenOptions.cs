using System;
using System.Collections.Generic;
using Microsoft.OpenApi.Models;
using Silky.Core.Utils;
using Silky.Swagger.Abstraction.SwaggerGen.SwaggerGenerator;

namespace Silky.Swagger.Gen.Configuration;

public class SilkySwaggerGenOptions
{
    internal static string SwaggerDocument = "SwaggerGen";

    public SilkySwaggerGenOptions()
    {
        Version = VersionHelper.GetCurrentVersion();
        EnableMultipleServiceKey = true;
        EnableHashRouteHeader = true;
        FilterTypes = new List<string>();
    }

    public bool EnableMultipleServiceKey { get; set; }

    public bool EnableHashRouteHeader { get; set; }

    public string Description { get; set; }
    public string Title { get; set; }
    public string Version { get; set; }

    public Uri TermsOfService { get; set; }

    public OpenApiContact Contact { get; set; }

    public IList<string> FilterTypes { get; set; }

    internal IList<Type> GetFilterTypes()
    {
        var types = new List<Type>();
        foreach (var filterTypeLine in FilterTypes)
        {
            var filterType = Type.GetType(filterTypeLine);
            if (typeof(IOperationFilter).IsAssignableFrom(filterType))
            {
                types.Add(filterType);
            }
            
        }

        return types;
    }

    public OpenApiLicense License { get; set; }
}