using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Silky.Core;
using Silky.Core.Utils;
using Silky.Swagger.Abstraction.SwaggerGen.SwaggerGenerator;
using IHostingEnvironment = Microsoft.AspNetCore.Hosting.IWebHostEnvironment;

namespace Silky.Swagger.Abstraction.SwaggerGen.DependencyInjection
{
    internal class ConfigureSwaggerGeneratorOptions : IConfigureOptions<SwaggerGeneratorOptions>
    {
        private readonly SwaggerGenOptions _swaggerGenOptions;
        private readonly IServiceProvider _serviceProvider;

        public ConfigureSwaggerGeneratorOptions(
            IOptions<SwaggerGenOptions> swaggerGenOptionsAccessor,
            IServiceProvider serviceProvider)
        {
            _swaggerGenOptions = swaggerGenOptionsAccessor.Value;
            _serviceProvider = serviceProvider;
        }

        public void Configure(SwaggerGeneratorOptions options)
        {
            DeepCopy(_swaggerGenOptions.SwaggerGeneratorOptions, options);

            // Generator and add any filters that were specified through the FilterDescriptor lists ...

            _swaggerGenOptions.ParameterFilterDescriptors.ForEach(
                filterDescriptor => options.ParameterFilters.Add(CreateFilter<IParameterFilter>(filterDescriptor)));

            _swaggerGenOptions.RequestBodyFilterDescriptors.ForEach(
                filterDescriptor => options.RequestBodyFilters.Add(CreateFilter<IRequestBodyFilter>(filterDescriptor)));

            _swaggerGenOptions.OperationFilterDescriptors.ForEach(
                filterDescriptor => options.OperationFilters.Add(CreateFilter<IOperationFilter>(filterDescriptor)));

            _swaggerGenOptions.DocumentFilterDescriptors.ForEach(
                filterDescriptor => options.DocumentFilters.Add(CreateFilter<IDocumentFilter>(filterDescriptor)));

            if (!options.SwaggerDocs.Any())
            {
                options.SwaggerDocs.Add(VersionHelper.GetCurrentVersion(),
                    new OpenApiInfo
                        { Title = EngineContext.Current.HostName, Version = VersionHelper.GetCurrentVersion() });
            }
        }
        
        public void DeepCopy(SwaggerGeneratorOptions source, SwaggerGeneratorOptions target)
        {
            target.SwaggerDocs = new Dictionary<string, OpenApiInfo>(source.SwaggerDocs);
            target.DocInclusionPredicate = source.DocInclusionPredicate;
            target.IgnoreObsoleteActions = source.IgnoreObsoleteActions;
            target.ConflictingActionsResolver = source.ConflictingActionsResolver;
            target.OperationIdSelector = source.OperationIdSelector;
            target.TagsSelector = source.TagsSelector;
            target.SortKeySelector = source.SortKeySelector;
            target.DescribeAllParametersInCamelCase = source.DescribeAllParametersInCamelCase;
            target.Servers = new List<OpenApiServer>(source.Servers);
            target.SecuritySchemes = new Dictionary<string, OpenApiSecurityScheme>(source.SecuritySchemes);
            target.SecurityRequirements = new List<OpenApiSecurityRequirement>(source.SecurityRequirements);
            target.ParameterFilters = new List<IParameterFilter>(source.ParameterFilters);
            target.OperationFilters = new List<IOperationFilter>(source.OperationFilters);
            target.DocumentFilters = new List<IDocumentFilter>(source.DocumentFilters);
        }

        private TFilter CreateFilter<TFilter>(FilterDescriptor filterDescriptor)
        {
            return (TFilter)ActivatorUtilities
                .CreateInstance(_serviceProvider, filterDescriptor.Type, filterDescriptor.Arguments);
        }
    }
}