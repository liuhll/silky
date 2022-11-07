using System;
using Silky.Core;
using Silky.Http.Swagger.Builders;
using Silky.Http.Swagger.Configuration;
using Silky.Swagger.Abstraction;
using Silky.Swagger.Abstraction.SwaggerUI;

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
        
    }
}