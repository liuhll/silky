using Microsoft.OpenApi.Models;
using Silky.Swagger.SwaggerGen.SwaggerGenerator;

namespace Silky.Swagger.SwaggerGen.Filters
{
    public class AddServiceKeyOperationFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            if (context.ServiceEntry.MultipleServiceKey)
            {
                operation.Parameters.Add(new OpenApiParameter()
                {
                    Name = "serviceKey",
                    In = ParameterLocation.Header,
                    Required = false,
                    Schema = new OpenApiSchema()
                    {
                        Type = "string"
                    },
                });
            }
        }
    }
}