using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Nacos.V2;
using Silky.Core.Extensions;
using Silky.Core.Serialization;
using Silky.RegistryCenter.Nacos.Configuration;
using Silky.Swagger.Abstraction;
using Silky.Swagger.Gen.Serialization;

namespace Silky.Swagger.Gen.Provider.Nacos;

public class NacosSwaggerInfoProvider : SwaggerInfoProviderBase, IRegisterCenterSwaggerInfoProvider
{
    private readonly INacosConfigService _nacosConfigService;
    private readonly ISwaggerSerializer _serializer;
    private readonly NacosRegistryCenterOptions _nacosRegistryCenterOptions;

    public NacosSwaggerInfoProvider(INacosConfigService nacosConfigService,
        ISwaggerSerializer serializer,
        IOptions<NacosRegistryCenterOptions> nacosRegistryCenterOptions)
    {
        _nacosConfigService = nacosConfigService;
        _serializer = serializer;
        _nacosRegistryCenterOptions = nacosRegistryCenterOptions.Value;
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
                await _nacosConfigService.GetConfig(documentName, _nacosRegistryCenterOptions.SwaggerDocGroupName, 10000);
            if (openApiDocumentValue.IsNullOrEmpty())
            {
                return null;
            }

            var openApiDocument = _serializer.Deserialize<OpenApiDocument>(openApiDocumentValue);
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
        foreach (var documentName in allDocumentNames)
        {
            var openApiDocument = await GetSwagger(documentName);
            if (openApiDocument != null)
            {
                openApiDocuments.Add(openApiDocument);
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
                    _nacosRegistryCenterOptions.SwaggerDocGroupName, timeoutMs);
            if (allDocumentsValue.IsNullOrEmpty())
            {
                return Array.Empty<string>();
            }

            return _serializer.Deserialize<string[]>(allDocumentsValue);
        }
        catch (Exception e)
        {
            return Array.Empty<string>();
        }
    }
}