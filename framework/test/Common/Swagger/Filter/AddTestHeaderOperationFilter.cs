using Microsoft.OpenApi.Models;
using Silky.Swagger.Abstraction.SwaggerGen.SwaggerGenerator;

namespace Common.Swagger.Filter;

public class AddTestHeaderOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        operation.Parameters.Add(new OpenApiParameter()
        {
            Name = "Test",
            In = ParameterLocation.Header,
            Required = false,
            Schema = new OpenApiSchema()
            {
                Type = "string"
            },
        });
    }
}