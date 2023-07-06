using System.IO;
using System.Text;
using Microsoft.OpenApi;
using Microsoft.OpenApi.Extensions;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Readers;

namespace Silky.Swagger.Gen.Extensions;

public static class OpenApiDocumentExtensions
{
    public static string ToJson(this OpenApiDocument document)
    {
        var outputString = document.Serialize(OpenApiSpecVersion.OpenApi3_0, OpenApiFormat.Json);
        return outputString;
    }

    public static OpenApiDocument ToOpenApiDocument(this string jsonString)
    {
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(jsonString));
        var openApiDocument = new OpenApiStreamReader().Read(stream, out var diagnostic);
        return openApiDocument;

    }
}