using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Silky.Core;
using Silky.Core.Modularity;
using Silky.Rpc;
using Silky.Rpc.Configuration;
using Silky.Swagger.Gen.Register;

namespace Silky.Swagger.Gen;

[DependsOn(typeof(RpcModule))]
public class SwaggerGenModule : SilkyModule
{
    public override void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddSwaggerRegisterInfoGen(configuration);
        services.AddSwaggerInfoService(configuration);
    }

    public override async Task Initialize(ApplicationInitializationContext context)
    {
        var registryCenterOptions = EngineContext.Current.Configuration.GetRegistryCenterOptions();
        if (registryCenterOptions.RegisterSwaggerDoc)
        {
            var swaggerInfoRegister = context.ServiceProvider.GetRequiredService<ISwaggerInfoRegister>();
            await swaggerInfoRegister.Register();
        }
    }
}