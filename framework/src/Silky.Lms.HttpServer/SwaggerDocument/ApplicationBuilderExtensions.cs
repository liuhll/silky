using Microsoft.AspNetCore.Builder;
using Silky.Lms.HttpServer.Configuration;

namespace Silky.Lms.HttpServer.SwaggerDocument
{
    internal static class ApplicationBuilderExtensions
    {
        public static IApplicationBuilder UseSwaggerDocuments(this IApplicationBuilder app,
            SwaggerDocumentOptions swaggerDocumentOptions)
        {
            
            // 配置 Swagger 全局参数
            app.UseSwagger(options => SwaggerDocumentBuilder.Build(options,swaggerDocumentOptions));

            // 配置 Swagger UI 参数
            app.UseSwaggerUI(options => SwaggerDocumentBuilder.BuildUI(options,swaggerDocumentOptions));

            return app;
        }
    }
}