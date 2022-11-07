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
            ParameterDescriptor bodyParameterDescription,
            IEnumerable<ParameterDescriptor> formParameterDescriptions,
            ISchemaGenerator schemaGenerator,
            SchemaRepository schemaRepository)
        {
            BodyParameterDescription = bodyParameterDescription;
            FormParameterDescriptions = formParameterDescriptions;
            SchemaGenerator = schemaGenerator;
            SchemaRepository = schemaRepository;
        }

        public ParameterDescriptor BodyParameterDescription { get; }

        public IEnumerable<ParameterDescriptor> FormParameterDescriptions { get; }

        public ISchemaGenerator SchemaGenerator { get; }

        public SchemaRepository SchemaRepository { get; }

        public string DocumentName => SchemaRepository.DocumentName;
    }
}