using System;
using IGeekFan.AspNetCore.Knife4jUI;
using Silky.Core;
using Silky.Http.Swagger.Builders;
using Silky.Http.Swagger.Configuration;
using Silky.Swagger;
using Silky.Swagger.SwaggerUI;

namespace Microsoft.AspNetCore.Builder
{
    public static class SwaggerApplicationBuilderExtensions
    {
        public static IApplicationBuilder UseSwaggerDocuments(this IApplicationBuilder app,
            Action<SwaggerOptions> swaggerSetupAction = null, Action<SwaggerUIOptions>
                swaggerUiSetupAction = null)
        {
            var swaggerDocumentOptions = EngineContext.Current.GetOptionsSnapshot<SwaggerDocumentOptions>();
            app.UseSwagger(swaggerSetupAction ??
                           (options => SwaggerDocumentBuilder.Build(options, swaggerDocumentOptions)));


            app.UseSwaggerUI(swaggerUiSetupAction ??
                             (options => SwaggerDocumentBuilder.BuildUI(options, swaggerDocumentOptions)));

            return app;
        }

        public static IApplicationBuilder UseKnife4jUIDocuments(this IApplicationBuilder app,
            Action<Knife4UIOptions> swaggerSetupAction = null, Action<Knife4UIOptions>
                swaggerUiSetupAction = null)
        {
            var swaggerDocumentOptions = EngineContext.Current.GetOptionsSnapshot<SwaggerDocumentOptions>();
            app.UseSwagger(options => SwaggerDocumentBuilder.Build(options, swaggerDocumentOptions));


            app.UseKnife4UI(swaggerUiSetupAction ??
                            (options => SwaggerDocumentBuilder.BuildKnife4jUI(options, swaggerDocumentOptions)));

            return app;
        }
    }
}