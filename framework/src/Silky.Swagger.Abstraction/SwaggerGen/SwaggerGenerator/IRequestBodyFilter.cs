using System.Collections.Generic;
using Microsoft.OpenApi.Models;
using Silky.Rpc.Runtime.Server;

namespace Silky.Swagger.Abstraction.SwaggerGen.SwaggerGenerator
{
    public interface IRequestBodyFilter
    {
        void Apply(OpenApiRequestBody requestBody, RequestBodyFilterContext context);
    }

    public class RequestBodyFilterContext
    {
        public RequestBodyFilterContext(
            RpcParameter bodyRpcParameterDescription,
            IEnumerable<RpcParameter> formParameterDescriptions,
            ISchemaGenerator schemaGenerator,
            SchemaRepository schemaRepository)
        {
            BodyRpcParameterDescription = bodyRpcParameterDescription;
            FormParameterDescriptions = formParameterDescriptions;
            SchemaGenerator = schemaGenerator;
            SchemaRepository = schemaRepository;
        }

        public RpcParameter BodyRpcParameterDescription { get; }

        public IEnumerable<RpcParameter> FormParameterDescriptions { get; }

        public ISchemaGenerator SchemaGenerator { get; }

        public SchemaRepository SchemaRepository { get; }

        public string DocumentName => SchemaRepository.DocumentName;
    }
}