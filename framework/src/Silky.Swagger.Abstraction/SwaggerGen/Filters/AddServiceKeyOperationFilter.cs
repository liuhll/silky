using Microsoft.OpenApi.Models;
using Silky.Rpc.Runtime.Server;
using Silky.Swagger.Abstraction.SwaggerGen.SwaggerGenerator;

namespace Silky.Swagger.Abstraction.SwaggerGen.Filters
{
    public class AddServiceKeyOperationFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            var serviceDescriptor = context.ServiceEntry.GetServiceDescriptor();
            if (serviceDescriptor != null && serviceDescriptor.MultiServiceKeys())
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