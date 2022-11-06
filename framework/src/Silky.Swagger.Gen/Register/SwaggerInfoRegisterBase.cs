using System.Threading.Tasks;
using Microsoft.OpenApi.Models;

namespace Silky.Swagger.Gen.Register;

public abstract class SwaggerInfoRegisterBase : ISwaggerInfoRegister
{
    private readonly ISwaggerProvider _swaggerProvider;

    protected SwaggerInfoRegisterBase(ISwaggerProvider swaggerProvider)
    {
        _swaggerProvider = swaggerProvider;
    }

    public async Task Register()
    {
        var swaggerInfos = _swaggerProvider.GetSwaggers();
        foreach (var swaggerInfo in swaggerInfos)
        {
            await Register(swaggerInfo.Key, swaggerInfo.Value);
        }
    }

    protected abstract Task Register(string documentName, OpenApiDocument openApiDocument);
}