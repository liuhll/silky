using Microsoft.AspNetCore.Builder;
using Silky.Http.Core.Configuration;

namespace Silky.Http.Core.SwaggerDocument
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