using System;
using System.Linq;
using Silky.Core;
using Silky.Http.Core.Middlewares;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Silky.Http.Core.Configuration;
using Silky.Http.Core.SwaggerDocument;
using Silky.Rpc.Gateway;
using Silky.Rpc.Routing;

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

        public async void Configure(IApplicationBuilder application)
        {
            GatewayOptions gatewayOption = default;
            gatewayOption = EngineContext.Current.GetOptionsMonitor<GatewayOptions>((options, name) =>
            {
                gatewayOption = options;
            });

            SwaggerDocumentOptions swaggerDocumentOptions = default;
            swaggerDocumentOptions = EngineContext.Current.GetOptionsMonitor<SwaggerDocumentOptions>(
                (options, name) => { swaggerDocumentOptions = options; });

            if (gatewayOption.EnableSwaggerDoc)
            {
                application.UseSwaggerDocuments(swaggerDocumentOptions);
            }

            application.UseHttpsRedirection();
            application.UseSilkyExceptionHandler();
            application.UseSilky();
            var gatewayManager = application.ApplicationServices.GetRequiredService<IGatewayManager>();
            
            await gatewayManager.RegisterGateway();
        }

        public int Order { get; } = Int32.MaxValue;
    }
}