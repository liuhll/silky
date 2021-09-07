using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Silky.Core;
using Silky.Core.Modularity;
using Silky.Http.Core;
using Silky.Http.Core.Configuration;
using Silky.Http.Swagger.Configuration;

namespace Silky.Http.Swagger
{
    [DependsOn(typeof(SilkyHttpCoreModule))]
    public class SwaggerModule : WebSilkyModule
    {
        public override void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            var enableSwaggerDoc = configuration.GetValue<bool?>("gateway:enableSwaggerDoc") ?? false;
            if (enableSwaggerDoc)
            {
                services.AddSwaggerDocuments();
            }

        }
        public override void Configure(IApplicationBuilder application)
        {
            var gatewayOptions = EngineContext.Current.GetOptionsSnapshot<GatewayOptions>();
            if (gatewayOptions.EnableSwaggerDoc)
            {
                var swaggerDocumentOptions = EngineContext.Current.GetOptionsSnapshot<SwaggerDocumentOptions>();
                application.UseSwaggerDocuments(swaggerDocumentOptions);
            }
           
        }
    }
}