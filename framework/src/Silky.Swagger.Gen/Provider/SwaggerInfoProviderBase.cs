using System.Threading.Tasks;

namespace Silky.Swagger.Gen.Provider;

public abstract class SwaggerInfoProviderBase : ISwaggerInfoProvider
{
    
    public abstract Task<string[]> GetGroups();
}