using System;
using System.Text.Json;
using Silky.Swagger.Abstraction;
using Silky.Swagger.Abstraction.SwaggerGen.DependencyInjection;
using Silky.Swagger.Abstraction.SwaggerGen.Filters;
using Silky.Swagger.Abstraction.SwaggerGen.SchemaGenerator;
using Silky.Swagger.Abstraction.SwaggerGen.SwaggerGenerator;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.serviceEntrys;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class SwaggerGenServiceCollectionExtensions
    {
        public static IServiceCollection AddSwaggerGen(
            this IServiceCollection services,
            Action<SwaggerGenOptions> setupAction = null)
        {
            // Add Mvc convention to ensure ApiExplorer is enabled for all actions
            services.Configure<MvcOptions>(c =>
                c.Conventions.Add(new SwaggerApplicationConvention()));

            // Register custom configurators that takes values from SwaggerGenOptions (i.e. high level config)
            // and applies them to SwaggerGeneratorOptions and SchemaGeneratorOptoins (i.e. lower-level config)
            services.AddTransient<IConfigureOptions<SwaggerGeneratorOptions>, ConfigureSwaggerGeneratorOptions>();
            services.AddTransient<IConfigureOptions<SchemaGeneratorOptions>, ConfigureSchemaGeneratorOptions>();

            // Register generator and it's dependencies
            services.TryAddTransient<ISwaggerProvider, SwaggerGenerator>();
            services.TryAddTransient(s => s.GetRequiredService<IOptions<SwaggerGeneratorOptions>>().Value);
            services.TryAddTransient<ISchemaGenerator, SchemaGenerator>();
            services.TryAddTransient(s => s.GetRequiredService<IOptions<SchemaGeneratorOptions>>().Value);
            services.TryAddTransient<ISerializerDataContractResolver>(s =>
            {
                var serializerOptions = s.GetJsonSerializerOptions() ?? new JsonSerializerOptions();
                return new JsonSerializerDataContractResolver(serializerOptions);
            });
            services.TryAddTransient<IOperationFilter, AddServiceKeyOperationFilter>();

            services.TryAddSingleton<IDocumentProvider, DocumentProvider>();

            if (setupAction != null) services.ConfigureSwaggerGen(setupAction);

            return services;
        }

        public static void ConfigureSwaggerGen(
            this IServiceCollection services,
            Action<SwaggerGenOptions> setupAction)
        {
            services.Configure(setupAction);
        }

        private static JsonSerializerOptions GetJsonSerializerOptions(this IServiceProvider serviceProvider)
        {
            return serviceProvider.GetService<IOptions<JsonOptions>>()?.Value?.JsonSerializerOptions;
        }
    }
}