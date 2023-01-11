using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using Silky.Core.Extensions;
using Silky.Rpc.Runtime.Server;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.OpenApi.Models;
using Silky.Core;
using Silky.Core.Extensions.Collections.Generic;
using Silky.Core.Logging;
using Silky.Core.Threading;

namespace Silky.Swagger.Abstraction.SwaggerGen.SwaggerGenerator
{
    public class SwaggerGenerator : ISwaggerProvider
    {
        private readonly IServiceEntryManager _serviceEntryManager;
        private readonly ISchemaGenerator _schemaGenerator;
        private readonly SwaggerGeneratorOptions _options;
        public ILogger<SwaggerGenerator> Logger { get; set; }

        public SwaggerGenerator(
            SwaggerGeneratorOptions options,
            ISchemaGenerator schemaGenerator,
            IServiceEntryManager serviceEntryManager)
        {
            _options = options ?? new SwaggerGeneratorOptions();
            _schemaGenerator = schemaGenerator;
            _serviceEntryManager = serviceEntryManager;
            Logger = NullLogger<SwaggerGenerator>.Instance;
        }


        public OpenApiDocument GetSwagger(string documentName,
            string host = null,
            string basePath = null,
            bool onlyLocalServices = false)
        {
            if (!_options.SwaggerDocs.TryGetValue(documentName, out OpenApiInfo info))
                throw new UnknownSwaggerDocument(documentName, _options.SwaggerDocs.Select(d => d.Key));

            var registerCenterSwaggerProvider = EngineContext.Current.Resolve<IRegisterCenterSwaggerInfoProvider>();
            if (onlyLocalServices || registerCenterSwaggerProvider == null)
            {
                return GetLocalSwagger(documentName, host, basePath, onlyLocalServices, info);
            }

            var openApiDocument =
                AsyncHelper.RunSync(() => registerCenterSwaggerProvider.GetSwagger(documentName));
            if (openApiDocument == null)
            {
                if (SwaggerGroupUtils.ReadLocalGroups().Contains(documentName))
                {
                    return GetLocalSwagger(documentName, host, basePath, onlyLocalServices, info);
                }

                var localOpenApiDocument = GetLocalSwagger(documentName, host, basePath, onlyLocalServices, info);

                var remoteOpenApiDocuments = AsyncHelper.RunSync(() => registerCenterSwaggerProvider.GetSwaggers());
                ;
                foreach (var remoteOpenApiDocument in remoteOpenApiDocuments)
                {
                    foreach (var path in remoteOpenApiDocument.Paths)
                    {
                        if (!localOpenApiDocument.Paths.ContainsKey(path.Key))
                        {
                            localOpenApiDocument.Paths.Add(path.Key, path.Value);
                        }
                    }

                    foreach (var schema in remoteOpenApiDocument.Components.Schemas)
                    {
                        if (!localOpenApiDocument.Components.Schemas.ContainsKey(schema.Key))
                        {
                            localOpenApiDocument.Components.Schemas.Add(schema.Key, schema.Value);
                        }
                    }
                }

                return localOpenApiDocument;
            }

            openApiDocument.Info = info;
            openApiDocument.Servers = GenerateServers(host, basePath);
            openApiDocument.SecurityRequirements = new List<OpenApiSecurityRequirement>(_options.SecurityRequirements);
            openApiDocument.Components.SecuritySchemes =
                new Dictionary<string, OpenApiSecurityScheme>(_options.SecuritySchemes);
            var schemaRepository = new SchemaRepository(documentName);
            var filterContext =
                new DocumentFilterContext(null, _schemaGenerator, schemaRepository);
            foreach (var filter in _options.DocumentFilters)
            {
                filter.Apply(openApiDocument, filterContext);
            }

            return openApiDocument;
        }

        private OpenApiDocument GetLocalSwagger(string documentName, string host, string basePath,
            bool onlyLocalServices, OpenApiInfo info)
        {
            var schemaRepository = new SchemaRepository(documentName);
            var entries = _serviceEntryManager.GetAllEntries()
                .Where(serviceEntry => !(_options.IgnoreObsoleteActions &&
                                         serviceEntry.CustomAttributes.OfType<ObsoleteAttribute>().Any()))
                .Where(serviceEntry => _options.DocInclusionPredicate(documentName, serviceEntry));
            if (onlyLocalServices)
            {
                entries = entries.Where(p => p.IsLocal);
            }

            var swaggerDoc = new OpenApiDocument
            {
                Info = info,
                Servers = GenerateServers(host, basePath),
                Paths = GeneratePaths(entries.ToList(), schemaRepository),
                Components = new OpenApiComponents
                {
                    Schemas = schemaRepository.Schemas,
                    SecuritySchemes = new Dictionary<string, OpenApiSecurityScheme>(_options.SecuritySchemes)
                },
                SecurityRequirements = new List<OpenApiSecurityRequirement>(_options.SecurityRequirements)
            };
            var filterContext =
                new DocumentFilterContext(entries, _schemaGenerator, schemaRepository);
            foreach (var filter in _options.DocumentFilters)
            {
                filter.Apply(swaggerDoc, filterContext);
            }

            return swaggerDoc;
        }

        private OpenApiPaths GeneratePaths(IReadOnlyList<ServiceEntry> entries, SchemaRepository schemaRepository)
        {
            var entriesByPath = entries.Where(p => !p.GovernanceOptions.ProhibitExtranet)
                .OrderBy(_options.SortKeySelector)
                .GroupBy(p => p.Router.RoutePath);

            var paths = new OpenApiPaths();
            foreach (var group in entriesByPath)
            {
                paths.Add($"/{group.Key}",
                    new OpenApiPathItem
                    {
                        Operations = GenerateOperations(group, schemaRepository)
                    });
            }

            return paths;
        }

        private IDictionary<OperationType, OpenApiOperation> GenerateOperations(
            IGrouping<string, ServiceEntry> serviceEntries, SchemaRepository schemaRepository)
        {
            var serviceEntriesByMethod = serviceEntries
                .OrderBy(_options.SortKeySelector)
                .GroupBy(apiDesc => apiDesc.Router.HttpMethod);
            var operations = new Dictionary<OperationType, OpenApiOperation>();
            foreach (var group in serviceEntriesByMethod)
            {
                var httpMethod = group.Key;

                if (httpMethod == null)
                    throw new SwaggerGeneratorException(string.Format(
                        "Ambiguous HTTP method for action - {0}. " +
                        "Actions require an explicit HttpMethod binding for Swagger/OpenAPI 3.0",
                        group.First().ServiceEntryDescriptor.Id));

                if (group.Count() > 1 && _options.ConflictingActionsResolver == null)
                    throw new SwaggerGeneratorException(string.Format(
                        "Conflicting method/path combination \"{0} {1}\" for actions - {2}. " +
                        "Actions require a unique method/path combination for Swagger/OpenAPI 3.0. Use ConflictingActionsResolver as a workaround",
                        httpMethod,
                        group.First().Router.RoutePath,
                        string.Join(",", group.Select(apiDesc => apiDesc.ServiceEntryDescriptor.Id))));

                var serviceEntry = (group.Count() > 1) ? _options.ConflictingActionsResolver(group) : group.Single();

                operations.Add(OperationTypeMap[httpMethod.ToString().ToUpper()],
                    GenerateOperation(serviceEntry, schemaRepository));
            }

            ;

            return operations;
        }

        private OpenApiOperation GenerateOperation(ServiceEntry serviceEntry, SchemaRepository schemaRepository)
        {
            try
            {
                var operation = new OpenApiOperation
                {
                    Tags = GenerateOperationTags(serviceEntry),
                    OperationId = _options.OperationIdSelector(serviceEntry),
                    Parameters = GenerateParameters(serviceEntry, schemaRepository),
                    RequestBody = GenerateRequestBody(serviceEntry, schemaRepository),
                    Responses = GenerateResponses(serviceEntry, schemaRepository),
                    Deprecated = serviceEntry.CustomAttributes.OfType<ObsoleteAttribute>().Any()
                };


                var methodInfo = serviceEntry.MethodInfo;
                var filterContext =
                    new OperationFilterContext(serviceEntry, _schemaGenerator, schemaRepository, methodInfo);
                foreach (var filter in _options.OperationFilters)
                {
                    filter.Apply(operation, filterContext);
                }

                return operation;
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
                throw new SwaggerGeneratorException(
                    message:
                    $"Failed to generate ServiceEntryId for action - {serviceEntry.ServiceEntryDescriptor.Id}. See inner exception",
                    innerException: ex);
            }
        }

        private OpenApiResponses GenerateResponses(ServiceEntry serviceEntry, SchemaRepository schemaRepository)
        {
            var responses = new OpenApiResponses();
            var allResponseCodes = ResponsesCodeHelper.GetAllCodes();
            foreach (var responseCode in allResponseCodes)
            {
                responses.Add(((int)responseCode.Key).ToString(),
                    GenerateResponse(serviceEntry, schemaRepository, responseCode));
            }

            // responses.Add(((int)ResponsesCode.Success).ToString(), GenerateResponse(serviceEntry, schemaRepository));
            return responses;
        }

        private OpenApiResponse GenerateResponse(ServiceEntry serviceEntry, SchemaRepository schemaRepository)
        {
            var description = ResponsesCode.Success.GetDisplay();
            var responseContentTypes = serviceEntry.SupportedResponseMediaTypes;
            return new OpenApiResponse
            {
                Description = description,
                Content = responseContentTypes.ToDictionary(
                    contentType => contentType,
                    contentType => CreateResponseMediaType(serviceEntry.ReturnType, schemaRepository)
                )
            };
        }


        private OpenApiResponse GenerateResponse(ServiceEntry serviceEntry, SchemaRepository schemaRepository,
            KeyValuePair<ResponsesCode, string> responseCodeSValuePair)
        {
            var description = responseCodeSValuePair.Value;
            var responseContentTypes = serviceEntry.SupportedResponseMediaTypes;
            if (responseCodeSValuePair.Key == ResponsesCode.Success)
            {
                return new OpenApiResponse
                {
                    Description = description,
                    Content = responseContentTypes.ToDictionary(
                        contentType => contentType,
                        contentType => CreateResponseMediaType(serviceEntry.ReturnType, schemaRepository)
                    )
                };
            }
            else
            {
                return new OpenApiResponse
                {
                    Description = description,
                    Content = responseContentTypes.ToDictionary(
                        contentType => contentType,
                        contentType => new OpenApiMediaType()
                    )
                };
            }
        }


        private OpenApiMediaType CreateResponseMediaType(Type returnType, SchemaRepository schemaRepository)
        {
            return new OpenApiMediaType
            {
                Schema = GenerateSchema(returnType, schemaRepository)
            };
        }

        private IEnumerable<string> InferResponseContentTypes(ServiceEntry serviceEntry,
            ApiResponseType apiResponseType)
        {
            return serviceEntry.SupportedResponseMediaTypes;
        }

        private OpenApiRequestBody GenerateRequestBody(ServiceEntry serviceEntry, SchemaRepository schemaRepository)
        {
            OpenApiRequestBody requestBody = null;
            RequestBodyFilterContext filterContext = null;
            var bodyParameter = serviceEntry.Parameters
                .FirstOrDefault(paramDesc => paramDesc.From == ParameterFrom.Body);

            var formParameters = serviceEntry.Parameters
                .Where(paramDesc => paramDesc.From == ParameterFrom.Form || paramDesc.From == ParameterFrom.File);
            if (bodyParameter != null)
            {
                requestBody = GenerateRequestBodyFromBodyParameter(serviceEntry, schemaRepository, bodyParameter);

                filterContext = new RequestBodyFilterContext(
                    bodyRpcParameterDescription: bodyParameter,
                    formParameterDescriptions: null,
                    schemaGenerator: _schemaGenerator,
                    schemaRepository: schemaRepository);
            }
            else if (formParameters.Any())
            {
                requestBody = GenerateRequestBodyFromFormParameters(serviceEntry, schemaRepository, formParameters);

                filterContext = new RequestBodyFilterContext(
                    bodyRpcParameterDescription: null,
                    formParameterDescriptions: formParameters,
                    schemaGenerator: _schemaGenerator,
                    schemaRepository: schemaRepository);
            }

            if (requestBody != null)
            {
                foreach (var filter in _options.RequestBodyFilters)
                {
                    filter.Apply(requestBody, filterContext);
                }
            }

            return requestBody;
        }

        private OpenApiRequestBody GenerateRequestBodyFromFormParameters(ServiceEntry serviceEntry,
            SchemaRepository schemaRepository, IEnumerable<RpcParameter> formParameters)
        {
            var contentTypes = InferRequestContentTypes(serviceEntry);
            contentTypes = contentTypes.Any() ? contentTypes : new[] { "multipart/form-data" };
            var schema = GenerateSchemaFromFormParameters(formParameters, schemaRepository);
            return new OpenApiRequestBody
            {
                Content = contentTypes
                    .ToDictionary(
                        contentType => contentType,
                        contentType => new OpenApiMediaType
                        {
                            Schema = schema,
                            Encoding = schema.Properties.ToDictionary(
                                entry => entry.Key,
                                entry => new OpenApiEncoding { Style = ParameterStyle.Form }
                            )
                        }
                    )
            };
        }

        private OpenApiSchema GenerateSchemaFromFormParameters(IEnumerable<RpcParameter> formParameters,
            SchemaRepository schemaRepository)
        {
            var properties = new Dictionary<string, OpenApiSchema>();
            var requiredPropertyNames = new List<string>();
            foreach (var formParameter in formParameters)
            {
                if (formParameter.IsSingleType)
                {
                    var name = _options.DescribeAllParametersInCamelCase
                        ? formParameter.Name.ToCamelCase()
                        : formParameter.Name;
                    var schema = GenerateSchema(
                        formParameter.Type,
                        schemaRepository,
                        null,
                        formParameter.ParameterInfo);

                    properties.Add(name, schema);

                    if (formParameter.Type.GetCustomAttributes()
                        .Any(attr => RequiredAttributeTypes.Contains(attr.GetType())))
                    {
                        requiredPropertyNames.Add(name);
                    }
                }
                else
                {
                    if (formParameter.IsFileParameter())
                    {
                        var name = _options.DescribeAllParametersInCamelCase
                            ? formParameter.Name.ToCamelCase()
                            : formParameter.Name;
                        var schema = GenerateSchema(
                            formParameter.Type,
                            schemaRepository,
                            null,
                            formParameter.ParameterInfo);
                        properties.Add(name, schema);
                        if (formParameter.ParameterInfo.GetCustomAttributes()
                            .Any(attr => RequiredAttributeTypes.Contains(attr.GetType())))
                        {
                            requiredPropertyNames.Add(name);
                        }
                    }
                    else
                    {
                        foreach (var propertyInfo in formParameter.Type.GetProperties())
                        {
                            var name = _options.DescribeAllParametersInCamelCase
                                ? propertyInfo.Name.ToCamelCase()
                                : propertyInfo.Name;
                            var schema = GenerateSchema(
                                formParameter.Type,
                                schemaRepository,
                                propertyInfo,
                                null);
                            properties.Add(name, schema);
                            if (propertyInfo.GetCustomAttributes()
                                .Any(attr => RequiredAttributeTypes.Contains(attr.GetType())))
                            {
                                requiredPropertyNames.Add(name);
                            }
                        }
                    }
                }
            }

            return new OpenApiSchema
            {
                Type = "object",
                Properties = properties,
                Required = new SortedSet<string>(requiredPropertyNames)
            };
        }

        private OpenApiRequestBody GenerateRequestBodyFromBodyParameter(ServiceEntry serviceEntry,
            SchemaRepository schemaRepository, RpcParameter bodyRpcParameter)
        {
            var contentTypes = InferRequestContentTypes(serviceEntry);
            var isRequired =
                bodyRpcParameter.Type.CustomAttributes.Any(attr => RequiredAttributeTypes.Contains(attr.GetType()));
            var schema = GenerateSchema(
                bodyRpcParameter.Type,
                schemaRepository,
                null,
                bodyRpcParameter.ParameterInfo);
            return new OpenApiRequestBody
            {
                Content = contentTypes
                    .ToDictionary(
                        contentType => contentType,
                        contentType => new OpenApiMediaType
                        {
                            Schema = schema,
                        }
                    ),
                Required = isRequired
            };
        }

        private IEnumerable<string> InferRequestContentTypes(ServiceEntry serviceEntry)
        {
            var explicitContentTypes = serviceEntry.CustomAttributes.OfType<ConsumesAttribute>()
                .SelectMany(attr => attr.ContentTypes)
                .Distinct();
            if (explicitContentTypes.Any()) return explicitContentTypes;
            var apiExplorerContentTypes = serviceEntry.SupportedRequestMediaTypes
                .Distinct();
            if (apiExplorerContentTypes.Any()) return apiExplorerContentTypes;

            return Enumerable.Empty<string>();
        }

        private IList<OpenApiParameter> GenerateParameters(ServiceEntry serviceEntry,
            SchemaRepository schemaRepository)
        {
            var applicableApiParameters = serviceEntry.Parameters
                .Where(apiParam =>
                    apiParam.From == ParameterFrom.Path ||
                    apiParam.From == ParameterFrom.Query ||
                    apiParam.From == ParameterFrom.Header
                );
            return applicableApiParameters
                .SelectMany(apiParam => GenerateParameter(apiParam, schemaRepository))
                .ToList();
        }

        private IEnumerable<OpenApiParameter> GenerateParameter(RpcParameter apiRpcParameter,
            SchemaRepository schemaRepository)
        {
            var parameters = new List<OpenApiParameter>();
            if (apiRpcParameter.IsSingleType)
            {
                parameters.Add(GenerateSampleParameter(apiRpcParameter, schemaRepository));
            }
            else
            {
                parameters.AddRange(GenerateComplexParameter(apiRpcParameter, schemaRepository));
            }

            return parameters;
        }


        private OpenApiParameter GenerateSampleParameter(RpcParameter apiRpcParameter,
            SchemaRepository schemaRespository)
        {
            var name = _options.DescribeAllParametersInCamelCase
                ? apiRpcParameter.Name.ToCamelCase()
                : apiRpcParameter.Name;

            var location = ParameterLocationMap[apiRpcParameter.From];
            var isRequired = (apiRpcParameter.From == ParameterFrom.Path)
                             || apiRpcParameter.Type.GetCustomAttributes()
                                 .Any(attr => RequiredAttributeTypes.Contains(attr.GetType()));

            var schema = GenerateSchema(apiRpcParameter.Type, schemaRespository, null,
                apiRpcParameter.ParameterInfo);
            var parameter = new OpenApiParameter
            {
                Name = location == ParameterLocation.Path ? name.ToLower() : name,
                In = location,
                Required = isRequired,
                Schema = schema
            };

            var filterContext = new ParameterFilterContext(
                apiRpcParameter,
                _schemaGenerator,
                schemaRespository,
                null,
                apiRpcParameter.ParameterInfo);

            foreach (var filter in _options.ParameterFilters)
            {
                filter.Apply(parameter, filterContext);
            }

            return parameter;
        }

        private IEnumerable<OpenApiParameter> GenerateComplexParameter(RpcParameter apiRpcParameter,
            SchemaRepository schemaRespository)
        {
            var parameters = new List<OpenApiParameter>();
            var propertyInfos = apiRpcParameter.Type.GetProperties();
            foreach (var propertyInfo in propertyInfos)
            {
                // if (!propertyInfo.PropertyType.IsSingleType())
                // {
                //     throw new SilkyException("Specifying QString parameters does not allow specifying complex type parameters");
                // }

                var name = apiRpcParameter.From == ParameterFrom.Path ? apiRpcParameter.Name : propertyInfo.Name;
                name = _options.DescribeAllParametersInCamelCase
                    ? name.ToCamelCase()
                    : name;
                var location = ParameterLocationMap[apiRpcParameter.From];
                var isRequired = (apiRpcParameter.From == ParameterFrom.Path)
                                 || apiRpcParameter.Type.GetCustomAttributes().Any(attr =>
                                     RequiredAttributeTypes.Contains(attr.GetType()));

                var schema = GenerateSchema(apiRpcParameter.Type, schemaRespository, propertyInfo,
                    apiRpcParameter.ParameterInfo);
                var parameter = new OpenApiParameter
                {
                    Name = name,
                    In = location,
                    Required = isRequired,
                    Schema = schema
                };

                var filterContext = new ParameterFilterContext(
                    apiRpcParameter,
                    _schemaGenerator,
                    schemaRespository,
                    null,
                    apiRpcParameter.ParameterInfo);

                foreach (var filter in _options.ParameterFilters)
                {
                    filter.Apply(parameter, filterContext);
                }

                parameters.Add(parameter);
            }

            return parameters;
        }


        private OpenApiSchema GenerateSchema(
            Type type,
            SchemaRepository schemaRepository,
            PropertyInfo propertyInfo = null,
            ParameterInfo parameterInfo = null)
        {
            try
            {
                return _schemaGenerator.GenerateSchema(type, schemaRepository, propertyInfo, parameterInfo);
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
                throw new SwaggerGeneratorException(
                    message: $"Failed to generate schema for type - {type}. See inner exception",
                    innerException: ex);
            }
        }

        private IList<OpenApiTag> GenerateOperationTags(ServiceEntry serviceEntry)
        {
            return _options.TagsSelector(serviceEntry)
                .Select(tagName => new OpenApiTag { Name = tagName })
                .ToList();
        }


        private IList<OpenApiServer> GenerateServers(string host, string basePath)
        {
            if (_options.Servers.Any())
            {
                return new List<OpenApiServer>(_options.Servers);
            }

            return (host == null && basePath == null)
                ? new List<OpenApiServer>()
                : new List<OpenApiServer> { new OpenApiServer { Url = $"{host}{basePath}" } };
        }

        private static readonly Dictionary<string, OperationType> OperationTypeMap =
            new Dictionary<string, OperationType>
            {
                { "GET", OperationType.Get },
                { "PUT", OperationType.Put },
                { "POST", OperationType.Post },
                { "DELETE", OperationType.Delete },
                { "OPTIONS", OperationType.Options },
                { "HEAD", OperationType.Head },
                { "PATCH", OperationType.Patch },
                { "TRACE", OperationType.Trace }
            };

        private static readonly Dictionary<ParameterFrom, ParameterLocation> ParameterLocationMap =
            new Dictionary<ParameterFrom, ParameterLocation>
            {
                { ParameterFrom.Query, ParameterLocation.Query },
                { ParameterFrom.Header, ParameterLocation.Header },
                { ParameterFrom.Path, ParameterLocation.Path }
            };

        private static readonly IEnumerable<Type> RequiredAttributeTypes = new[]
        {
            typeof(BindRequiredAttribute),
            typeof(RequiredAttribute)
        };
    }
}