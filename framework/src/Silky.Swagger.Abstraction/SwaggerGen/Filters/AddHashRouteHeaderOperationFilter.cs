using Microsoft.OpenApi.Models;
using Silky.Rpc.Endpoint.Selector;
using Silky.Rpc.Runtime;
using Silky.Swagger.Abstraction.SwaggerGen.SwaggerGenerator;

namespace Silky.Swagger.Abstraction.SwaggerGen.Filters;

public class AddHashRouteHeaderOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        if (context.ServiceEntry.GovernanceOptions.ShuntStrategy == ShuntStrategy.HashAlgorithm)
        {
            operation.Parameters.Add(new OpenApiParameter()
            {
                Name = HashShuntStrategyAttribute.HashKey,
                In = ParameterLocation.Header,
                Required = true,
                Schema = new OpenApiSchema()
                {
                    Type = "string"
                },
            });
        }
    }
}