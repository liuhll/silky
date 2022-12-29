using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Nacos.V2;
using Newtonsoft.Json;
using Silky.Core.Exceptions;
using Silky.Core.Extensions;
using Silky.Core.Serialization;
using Silky.RegistryCenter.Nacos.Configuration;
using Silky.Swagger.Abstraction;

namespace Silky.Swagger.Gen.Register.Nacos;

public class NacosSwaggerInfoRegister : SwaggerInfoRegisterBase
{
    private readonly INacosConfigService _nacosConfigService;
    private readonly ISerializer _serializer;
    private readonly NacosRegistryCenterOptions _nacosRegistryCenterOptions;

    public NacosSwaggerInfoRegister(ISwaggerProvider swaggerProvider,
        INacosConfigService nacosConfigService,
        ISerializer serializer,
        IOptions<NacosRegistryCenterOptions> nacosRegistryCenterOptions) : base(swaggerProvider)
    {
        _nacosConfigService = nacosConfigService;
        _serializer = serializer;
        _nacosRegistryCenterOptions = nacosRegistryCenterOptions.Value;
    }

    protected override async Task Register(string documentName, OpenApiDocument openApiDocument)
    {
        var allDocumentNames = await GetAllDocumentNames();
        if (!allDocumentNames.Contains(documentName))
        {
            var registerDocumentNames = allDocumentNames.Concat(new[] { documentName });
            var registerDocumentNamesValue = _serializer.Serialize(registerDocumentNames, typeNameHandling: TypeNameHandling.Auto);
            var documentNamePublishResult = await _nacosConfigService.PublishConfig(
                _nacosRegistryCenterOptions.SwaggerDocKey,
                _nacosRegistryCenterOptions.ServerGroupName,
                registerDocumentNamesValue);
            if (!documentNamePublishResult)
            {
                throw new SilkyException($"swagger group {documentName} registration failed");
            }
        }

        var openApiDocumentValue = _serializer.Serialize(openApiDocument, typeNameHandling: TypeNameHandling.Auto);
        var openApiDocumentResult = await _nacosConfigService.PublishConfig(documentName,
            _nacosRegistryCenterOptions.ServerGroupName,
            openApiDocumentValue);
        if (!openApiDocumentResult)
        {
            throw new SilkyException($"Failed to publish {documentName} OpenApiDocument");
        }
    }

    private async Task<string[]> GetAllDocumentNames(int timeoutMs = 10000)
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

            return _serializer.Deserialize<string[]>(allDocumentsValue, typeNameHandling: TypeNameHandling.Auto);
        }
        catch (Exception e)
        {
            return Array.Empty<string>();
        }
    }
}