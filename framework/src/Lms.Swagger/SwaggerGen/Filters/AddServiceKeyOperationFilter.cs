using Lms.Swagger.SwaggerGen.SwaggerGenerator;
using Microsoft.OpenApi.Models;

namespace Lms.Swagger.SwaggerGen.Filters
{
    public class AddServiceKeyOperationFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            if (context.ApiDescription.MultipleServiceKey)
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