using Microsoft.OpenApi.Models;
using Silky.Swagger.Abstraction.SwaggerGen.SwaggerGenerator;

namespace GatewayDemo.Swagger.Filter;

public class AddTestHeaderOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        operation.Parameters.Add(new OpenApiParameter()
        {
            Name = "Test",
            In = ParameterLocation.Header,
            Required = true,
            Schema = new OpenApiSchema()
            {
                Type = "string"
            },
        });
    }
}