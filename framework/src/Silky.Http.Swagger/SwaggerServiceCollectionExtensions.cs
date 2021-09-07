using Microsoft.Extensions.DependencyInjection;
using Silky.Core;
using Silky.Http.Swagger.Builders;
using Silky.Http.Swagger.Configuration;

namespace Microsoft.AspNetCore.Builder
{
    internal static class SwaggerServiceCollectionExtensions
    {
        public static IServiceCollection AddSwaggerDocuments(this IServiceCollection services)
        {
            services.AddOptions<SwaggerDocumentOptions>()
                .Bind(EngineContext.Current.Configuration.GetSection(SwaggerDocumentOptions.SwaggerDocument));
            services.AddSwaggerGen(options => SwaggerDocumentBuilder.BuildGen(options, EngineContext.Current.Configuration));
            return services;
        }
    }
}