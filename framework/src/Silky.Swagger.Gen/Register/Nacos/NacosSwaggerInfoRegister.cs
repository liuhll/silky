using System;
using System.Linq;
using System.Threading.Tasks;
using Medallion.Threading;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Nacos.V2;
using Silky.Core.Exceptions;
using Silky.Core.Extensions;
using Silky.Core.Serialization;
using Silky.RegistryCenter.Nacos.Configuration;
using Silky.Swagger.Abstraction;
using Silky.Swagger.Gen.Serialization;

namespace Silky.Swagger.Gen.Register.Nacos;

public class NacosSwaggerInfoRegister : SwaggerInfoRegisterBase
{
    private readonly INacosConfigService _nacosConfigService;
    private readonly ISwaggerSerializer _serializer;
    private readonly NacosRegistryCenterOptions _nacosRegistryCenterOptions;
    private readonly IDistributedLockProvider _distributedLockProvider;

    public NacosSwaggerInfoRegister(ISwaggerProvider swaggerProvider,
        INacosConfigService nacosConfigService,
        ISwaggerSerializer serializer, IDistributedLockProvider distributedLockProvider,
        IOptions<NacosRegistryCenterOptions> nacosRegistryCenterOptions) : base(swaggerProvider)
    {
        _nacosConfigService = nacosConfigService;
        _serializer = serializer;
        _distributedLockProvider = distributedLockProvider;
        _nacosRegistryCenterOptions = nacosRegistryCenterOptions.Value;
    }

    protected override async Task Register(string documentName, OpenApiDocument openApiDocument)
    {
        await using var handle =
            await _distributedLockProvider.TryAcquireLockAsync($"RegisterSwaggerForNacos:{documentName}");
        if (handle != null)
        {
            var allDocumentNames = await GetAllDocumentNames();
            if (!allDocumentNames.Contains(documentName))
            {
                var registerDocumentNames = allDocumentNames.Concat(new[] { documentName });
                var registerDocumentNamesValue = _serializer.Serialize(registerDocumentNames);
                var documentNamePublishResult = await _nacosConfigService.PublishConfig(
                    _nacosRegistryCenterOptions.SwaggerDocKey,
                    _nacosRegistryCenterOptions.ServerGroupName,
                    registerDocumentNamesValue);
                if (!documentNamePublishResult)
                {
                    throw new SilkyException($"swagger group {documentName} registration failed");
                }
            }

            var openApiDocumentValue = _serializer.Serialize(openApiDocument);
            var openApiDocumentResult = await _nacosConfigService.PublishConfig(documentName,
                _nacosRegistryCenterOptions.ServerGroupName,
                openApiDocumentValue);
            if (!openApiDocumentResult)
            {
                throw new SilkyException($"Failed to publish {documentName} OpenApiDocument");
            }
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

            return _serializer.Deserialize<string[]>(allDocumentsValue);
        }
        catch (Exception e)
        {
            return Array.Empty<string>();
        }
    }
}