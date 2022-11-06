using System.Threading.Tasks;

namespace Silky.Swagger.Gen.Provider;

public interface ISwaggerInfoProvider
{
    
    Task<string[]> GetGroups();
}