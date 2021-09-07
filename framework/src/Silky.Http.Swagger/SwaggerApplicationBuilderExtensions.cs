using Silky.Http.Swagger.Builders;
using Silky.Http.Swagger.Configuration;

namespace Microsoft.AspNetCore.Builder
{
    internal static class SwaggerApplicationBuilderExtensions
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