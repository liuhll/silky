using System.Collections.Generic;
using System.Linq;
using Microsoft.OpenApi.Models;
using Silky.Swagger.Abstraction;

namespace Silky.Swagger.Gen;

public static class SwaggerProviderExtensions
{
    public static IDictionary<string, OpenApiDocument> GetLocalSwaggers(this ISwaggerProvider swaggerProvider)
    {
        var swaggerDictionary = new Dictionary<string, OpenApiDocument>();
        var groups = SwaggerGroupUtils.ReadLocalGroups();
        foreach (var group in groups)
        {
            var openApiDocument = swaggerProvider.GetSwagger(group, onlyLocalServices: true);
            if (!openApiDocument.Paths.Any())
            {
                continue;
            }

            swaggerDictionary.Add(group, openApiDocument);
        }

        return swaggerDictionary;
    }
}