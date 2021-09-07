using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Silky.Core;
using Silky.Core.Modularity;
using Silky.Http.Core;
using Silky.Http.Swagger.Configuration;

namespace Silky.Http.Swagger
{
    [DependsOn(typeof(SilkyHttpCoreModule))]
    public class SwaggerModule : WebSilkyModule
    {
        public override void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            services.AddSwaggerDocuments();

        }
        public override void Configure(IApplicationBuilder application)
        {
            var swaggerDocumentOptions = EngineContext.Current.GetOptionsSnapshot<SwaggerDocumentOptions>();
            application.UseSwaggerDocuments(swaggerDocumentOptions);
           
        }
    }
}