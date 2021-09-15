using Microsoft.OpenApi.Models;
using Silky.Rpc.Routing;
using Silky.Rpc.Runtime.Server;
using Silky.Swagger.SwaggerGen.SwaggerGenerator;

namespace Silky.Swagger.SwaggerGen.Filters
{
    public class AddServiceKeyOperationFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            var serviceRoute = context.ServiceEntry.GetServiceRoute();
            if (serviceRoute != null && serviceRoute.MultiServiceKeys())
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