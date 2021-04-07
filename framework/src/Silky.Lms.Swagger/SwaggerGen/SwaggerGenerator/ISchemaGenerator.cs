using System;
using System.Reflection;
using Microsoft.OpenApi.Models;

namespace Silky.Lms.Swagger.SwaggerGen.SwaggerGenerator
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
