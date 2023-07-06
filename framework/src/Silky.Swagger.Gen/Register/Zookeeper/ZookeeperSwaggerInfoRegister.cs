using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Writers;
using Newtonsoft.Json;
using Silky.Core;
using Silky.Core.Extensions;
using Silky.Core.Serialization;
using Silky.RegistryCenter.Zookeeper;
using Silky.RegistryCenter.Zookeeper.Configuration;
using Silky.Swagger.Abstraction;
using Silky.Swagger.Gen.Extensions;
using Silky.Zookeeper;

namespace Silky.Swagger.Gen.Register.Zookeeper;

internal class ZookeeperSwaggerInfoRegister : SwaggerInfoRegisterBase
{
    private readonly IZookeeperClientFactory _zookeeperClientFactory;
    private ZookeeperRegistryCenterOptions _registryCenterOptions;

    public ILogger<ZookeeperSwaggerInfoRegister> Logger { get; set; }

    public ZookeeperSwaggerInfoRegister(ISwaggerProvider swaggerProvider,
        IZookeeperClientFactory zookeeperClientFactory,
        IOptions<ZookeeperRegistryCenterOptions> registryCenterOptions) : base(
        swaggerProvider)
    {
        _zookeeperClientFactory = zookeeperClientFactory;
   
        _registryCenterOptions = registryCenterOptions.Value;
        Check.NotNullOrEmpty(_registryCenterOptions.SwaggerDocPath, nameof(_registryCenterOptions.SwaggerDocPath));
        Logger = NullLogger<ZookeeperSwaggerInfoRegister>.Instance;
    }

    protected override async Task Register(string documentName, OpenApiDocument openApiDocument)
    {
        var zookeeperClients = _zookeeperClientFactory.GetZooKeeperClients();
        var swaggerDocPath = CreateSwaggerDocPath(documentName);
        foreach (var zookeeperClient in zookeeperClients)
        {
            // await CreateSubscribeServersChange(zookeeperClient);
        
            var routePath = _registryCenterOptions.SwaggerDocPath;
            if (!await zookeeperClient.ExistsAsync(routePath))
            {
                await zookeeperClient.Authorize(_registryCenterOptions.Scheme, _registryCenterOptions.Auth);
                await zookeeperClient.CreateRecursiveAsync(routePath, null,
                    AclUtils.GetAcls(_registryCenterOptions.Scheme, _registryCenterOptions.Auth));
            }
            var jsonString = openApiDocument.ToJson();
            var data = jsonString.GetBytes();
            if (!await zookeeperClient.ExistsAsync(swaggerDocPath))
            {
                await zookeeperClient.Authorize(_registryCenterOptions.Scheme, _registryCenterOptions.Auth);
                await zookeeperClient.CreateRecursiveAsync(swaggerDocPath, data,
                    AclUtils.GetAcls(_registryCenterOptions.Scheme, _registryCenterOptions.Auth));
                Logger.LogDebug($"Node {swaggerDocPath} does not exist and will be created");
            }
            else
            {
                await zookeeperClient.Authorize(_registryCenterOptions.Scheme, _registryCenterOptions.Auth);
                await zookeeperClient.SetDataAsync(swaggerDocPath, data);
                Logger.LogDebug($"The cached swaggerdocInfo data of the {swaggerDocPath} node has been updated");
            }
        }
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