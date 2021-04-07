using System.Reflection;
using Silky.Lms.Rpc.Runtime.Server;
using Microsoft.OpenApi.Models;

namespace Silky.Lms.Swagger.SwaggerGen.SwaggerGenerator
{
    public interface IOperationFilter
    {
        void Apply(OpenApiOperation operation, OperationFilterContext context);
    }

    public class OperationFilterContext
    {
        public OperationFilterContext(
            ServiceEntry apiDescription,
            ISchemaGenerator schemaRegistry,
            SchemaRepository schemaRepository,
            MethodInfo methodInfo)
        {
            ApiDescription = apiDescription;
            SchemaGenerator = schemaRegistry;
            SchemaRepository = schemaRepository;
            MethodInfo = methodInfo;
        }

        public ServiceEntry ApiDescription { get; }

        public ISchemaGenerator SchemaGenerator { get; }

        public SchemaRepository SchemaRepository { get; }

        public MethodInfo MethodInfo { get; }

        public string DocumentName => SchemaRepository.DocumentName;
    }
}
