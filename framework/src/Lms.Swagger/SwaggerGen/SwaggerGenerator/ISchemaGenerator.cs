using System;
using System.Reflection;
using Microsoft.OpenApi.Models;

namespace Lms.Swagger.SwaggerGen.SwaggerGenerator
{
    public interface ISchemaGenerator
    {
        OpenApiSchema GenerateSchema(
            Type type,
            SchemaRepository schemaRepository,
            MemberInfo memberInfo = null,
            ParameterInfo parameterInfo = null);
    }
}
