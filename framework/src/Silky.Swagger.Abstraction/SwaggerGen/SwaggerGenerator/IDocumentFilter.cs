using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.OpenApi.Models;
using Silky.Rpc.Runtime.Server;

namespace Silky.Swagger.Abstraction.SwaggerGen.SwaggerGenerator
{
    public interface IDocumentFilter
    {
        void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context);
    }

    public class DocumentFilterContext
    {
        public DocumentFilterContext(
            IEnumerable<ServiceEntry> serviceEntries,
            ISchemaGenerator schemaGenerator,
            SchemaRepository schemaRepository)
        {
            ServiceEntries = serviceEntries;
            SchemaGenerator = schemaGenerator;
            SchemaRepository = schemaRepository;
        }

        public IEnumerable<ServiceEntry> ServiceEntries { get; }

        public ISchemaGenerator SchemaGenerator { get; }

        public SchemaRepository SchemaRepository { get; }

        public string DocumentName => SchemaRepository.DocumentName;
    }
}