using System;
using Silky.Core;
using Silky.Http.Core.Middlewares;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Silky.Http.Core.Configuration;
using Silky.Http.Core.SwaggerDocument;

namespace Silky.Http.Core
{
    public class HttpServerSilkyStartup : ISilkyStartup
    {
        public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            services.AddOptions<GatewayOptions>()
                .Bind(configuration.GetSection(GatewayOptions.Gateway));
            services.AddOptions<SwaggerDocumentOptions>()
                .Bind(configuration.GetSection(SwaggerDocumentOptions.SwaggerDocument));
            
            services.AddSwaggerDocuments(configuration);
        }

        public void Configure(IApplicationBuilder application)
        {
            var gatewayOption = EngineContext.Current.GetOptions<GatewayOptions>();

            var swaggerDocumentOptions = EngineContext.Current.GetOptions<SwaggerDocumentOptions>();

            if (gatewayOption.EnableSwaggerDoc)
            {
                application.UseSwaggerDocuments(swaggerDocumentOptions);
            }
            
            application.UseSilkyExceptionHandler();
            application.UseSilky();
        }

        public int Order { get; } = Int32.MinValue;
    }
}