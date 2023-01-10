using System.Reflection;
using Microsoft.OpenApi.Models;
using Silky.Rpc.Runtime.Server;

namespace Silky.Swagger.Abstraction.SwaggerGen.SwaggerGenerator
{
    public interface IParameterFilter
    {
        void Apply(OpenApiParameter parameter, ParameterFilterContext context);
    }

    public class ParameterFilterContext
    {
        public ParameterFilterContext(
            RpcParameter apiRpcParameterDescription,
            ISchemaGenerator schemaGenerator,
            SchemaRepository schemaRepository,
            PropertyInfo propertyInfo = null,
            ParameterInfo parameterInfo = null)
        {
            ApiRpcParameterDescription = apiRpcParameterDescription;
            SchemaGenerator = schemaGenerator;
            SchemaRepository = schemaRepository;
            PropertyInfo = propertyInfo;
            ParameterInfo = parameterInfo;
        }

        public RpcParameter ApiRpcParameterDescription { get; }

        public ISchemaGenerator SchemaGenerator { get; }

        public SchemaRepository SchemaRepository { get; }

        public PropertyInfo PropertyInfo { get; }

        public ParameterInfo ParameterInfo { get; }

        public string DocumentName => SchemaRepository.DocumentName;
    }
}