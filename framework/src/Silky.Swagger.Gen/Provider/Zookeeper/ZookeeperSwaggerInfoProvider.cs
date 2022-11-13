using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Silky.Core;
using Silky.Core.Extensions;
using Silky.Core.Serialization;
using Silky.RegistryCenter.Zookeeper;
using Silky.RegistryCenter.Zookeeper.Configuration;
using Silky.Swagger.Abstraction;
using Silky.Swagger.Gen.Serialization;
using Silky.Zookeeper;

namespace Silky.Swagger.Gen.Provider.Zookeeper;

public class ZookeeperSwaggerInfoProvider : SwaggerInfoProviderBase, IRegisterCenterSwaggerInfoProvider
{
    private readonly IZookeeperClientFactory _zookeeperClientFactory;
    private ZookeeperRegistryCenterOptions _registryCenterOptions;
    private readonly ISwaggerSerializer _serializer;

    private static readonly string RouteTemplate = "/swagger/{documentName}/swagger.json";

    public ILogger<ZookeeperSwaggerInfoProvider> Logger { get; set; }


    public ZookeeperSwaggerInfoProvider(IZookeeperClientFactory zookeeperClientFactory,
        ISwaggerSerializer serializer,
        IOptions<ZookeeperRegistryCenterOptions> registryCenterOptions)
    {
        _zookeeperClientFactory = zookeeperClientFactory;
        _serializer = serializer;
        _registryCenterOptions = registryCenterOptions.Value;
        Check.NotNullOrEmpty(_registryCenterOptions.SwaggerDocPath, nameof(_registryCenterOptions.SwaggerDocPath));
        Logger = NullLogger<ZookeeperSwaggerInfoProvider>.Instance;
    }

    public override async Task<string[]> GetGroups()
    {
        var zookeeperClient = _zookeeperClientFactory.GetZooKeeperClient();
        return await GetDocuments(zookeeperClient);
    }


    public async Task<OpenApiDocument> GetSwagger(string documentName)
    {
        var zookeeperClient = _zookeeperClientFactory.GetZooKeeperClient();
        return await GetSwagger(documentName, zookeeperClient);
    }

    public async Task<OpenApiDocument> GetSwagger(string documentName, IZookeeperClient zookeeperClient)
    {
        var documentPath = CreateSwaggerDocPath(documentName);
        if (!await zookeeperClient.ExistsAsync(documentPath))
        {
            return null;
        }
        await zookeeperClient.Authorize(_registryCenterOptions.Scheme, _registryCenterOptions.Auth);
        var datas = await zookeeperClient.GetDataAsync(documentPath);
        if (datas == null || !datas.Any())
        {
            return null;
        }

        var jsonString = datas.ToArray().GetString();

        return _serializer.Deserialize<OpenApiDocument>(jsonString);
    }


    public async Task<IEnumerable<OpenApiDocument>> GetSwaggers()
    {
        var openApiDocuments = new List<OpenApiDocument>();
        var zookeeperClient = _zookeeperClientFactory.GetZooKeeperClient();
        var documents = await GetDocuments(zookeeperClient);
        foreach (var document in documents)
        {
            var openApiDocument = await GetSwagger(document, zookeeperClient);
            openApiDocuments.Add(openApiDocument);
        }

        return openApiDocuments;
    }


    // private async Task UpdateSwaggerEndpoint(IZookeeperClient zookeeperClient)
    // {
    //     var swaggerUIOptions = EngineContext.Current.GetOptions<SwaggerUIOptions>();
    //     var documents = await GetDocuments(zookeeperClient);
    //     foreach (var document in documents)
    //     {
    //         var routeTemplate =
    //             RouteTemplate.Replace("{documentName}", Uri.EscapeDataString(document));
    //         swaggerUIOptions.AddIfNotContainsSwaggerEndpoint(routeTemplate, document);
    //     }
    // }

    private async Task<string[]> GetDocuments(IZookeeperClient zookeeperClient)
    {
      
        if (!await zookeeperClient.ExistsAsync(_registryCenterOptions.SwaggerDocPath))
        {
            return Array.Empty<string>();
        }
        await zookeeperClient.Authorize(_registryCenterOptions.Scheme, _registryCenterOptions.Auth);
        var children = await zookeeperClient.GetChildrenAsync(_registryCenterOptions.SwaggerDocPath);
        if (children == null)
        {
            return Array.Empty<string>();
        }

        var groups = children.ToArray();
        return groups;
    }


    private string CreateSwaggerDocPath(string child)
    {
        var routePath = _registryCenterOptions.SwaggerDocPath;
        if (!routePath.EndsWith("/"))
        {
            routePath += "/";
        }

        routePath += child;
        return routePath;
    }
}