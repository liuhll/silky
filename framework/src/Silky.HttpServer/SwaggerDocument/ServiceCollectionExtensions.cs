using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Silky.HttpServer.SwaggerDocument
{
    internal static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddSwaggerDocuments(this IServiceCollection services,
            IConfiguration configuration)
        {
            var enableSwaggerDoc = configuration.GetValue<bool?>("gateway:enableSwaggerDoc") ?? false;
            if (!enableSwaggerDoc) return services;

            services.AddSwaggerGen(options => SwaggerDocumentBuilder.BuildGen(options, configuration));
            return services;
        }
    }
}