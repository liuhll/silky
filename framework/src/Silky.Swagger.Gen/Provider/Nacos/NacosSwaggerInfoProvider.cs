using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Nacos.V2;
using Newtonsoft.Json;
using Silky.Core.Extensions;
using Silky.Core.Serialization;
using Silky.RegistryCenter.Nacos.Configuration;
using Silky.Swagger.Abstraction;

namespace Silky.Swagger.Gen.Provider.Nacos;

public class NacosSwaggerInfoProvider : SwaggerInfoProviderBase, IRegisterCenterSwaggerInfoProvider
{
    private readonly INacosConfigService _nacosConfigService;
    private readonly ISerializer _serializer;
    private readonly NacosRegistryCenterOptions _nacosRegistryCenterOptions;

    public ILogger<NacosSwaggerInfoProvider> Logger { get; set; }

    public NacosSwaggerInfoProvider(INacosConfigService nacosConfigService,
        ISerializer serializer,
        IOptions<NacosRegistryCenterOptions> nacosRegistryCenterOptions)
    {
        _nacosConfigService = nacosConfigService;
        _serializer = serializer;
        _nacosRegistryCenterOptions = nacosRegistryCenterOptions.Value;
        Logger = NullLogger<NacosSwaggerInfoProvider>.Instance;
    }

    public override async Task<string[]> GetGroups()
    {
        return await GetAllDocuments();
    }

    public async Task<OpenApiDocument> GetSwagger(string documentName)
    {
        try
        {
            var openApiDocumentValue =
                await _nacosConfigService.GetConfig(documentName, _nacosRegistryCenterOptions.ServerGroupName, 10000);
            if (openApiDocumentValue.IsNullOrEmpty())
            {
                return null;
            }

            var openApiDocument = _serializer.Deserialize<OpenApiDocument>(openApiDocumentValue, camelCase: false,
                typeNameHandling: TypeNameHandling.Auto);
            return openApiDocument;
        }
        catch (Exception e)
        {
            return null;
        }
    }

    public async Task<IEnumerable<OpenApiDocument>> GetSwaggers()
    {
        var openApiDocuments = new List<OpenApiDocument>();

        var allDocumentNames = await GetAllDocuments();
        foreach (var document in allDocumentNames)
        {
            try
            {
                var openApiDocument = await GetSwagger(document);
                if (openApiDocument != null)
                {
                    openApiDocuments.Add(openApiDocument);
                }
            }
            catch (Exception e)
            {
                Logger.LogWarning($"Failed to fetch {document} openApiDocument from service registry");
            }
        }

        return openApiDocuments;
    }

    private async Task<string[]> GetAllDocuments(int timeoutMs = 10000)
    {
        try
        {
            var allDocumentsValue =
                await _nacosConfigService.GetConfig(_nacosRegistryCenterOptions.SwaggerDocKey,
                    _nacosRegistryCenterOptions.ServerGroupName, timeoutMs);
            if (allDocumentsValue.IsNullOrEmpty())
            {
                return Array.Empty<string>();
            }

            return _serializer.Deserialize<string[]>(allDocumentsValue, camelCase: false,
                typeNameHandling: TypeNameHandling.Auto);
        }
        catch (Exception e)
        {
            return Array.Empty<string>();
        }
    }
}