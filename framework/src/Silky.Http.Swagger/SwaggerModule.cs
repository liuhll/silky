using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Silky.Core.Modularity;
using Silky.Http.Core;

namespace Silky.Http.Swagger
{
    [DependsOn(typeof(SilkyHttpCoreModule))]
    public class SwaggerModule : HttpSilkyModule
    {
        public override void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            services.AddSwaggerDocuments();
            services.AddSwaggerInfoService(configuration);
        }

        public override void Configure(IApplicationBuilder application)
        {
            application.UseSwaggerDocuments();
        }
    }
}