using System;
using Silky.Core;
using Silky.Core.DependencyInjection;
using Silky.Http.Swagger.Builders;
using Silky.Http.Swagger.Configuration;
using Silky.Swagger.Abstraction;
using Silky.Swagger.Abstraction.SwaggerGen.DependencyInjection;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class SwaggerServiceCollectionExtensions
    {
        public static IServiceCollection AddSwaggerDocuments(this IServiceCollection services,
            Action<SwaggerGenOptions> setupAction = null)
        {
            if (!services.IsAdded(typeof(ISwaggerProvider)))
            {
                services.AddOptions<SwaggerDocumentOptions>()
                    .Bind(EngineContext.Current.Configuration.GetSection(SwaggerDocumentOptions.SwaggerDocument));

                services.AddSwaggerGen(setupAction ?? (options =>
                    SwaggerDocumentBuilder.BuildGen(options, EngineContext.Current.Configuration)));
            }
            
            return services;
        }
        
    }
}