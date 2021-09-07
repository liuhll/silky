using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Silky.Http.Swagger.Builders;

namespace Silky.Http.Swagger
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