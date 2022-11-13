using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Silky.Swagger.Gen.Serialization;

public class SwaggerSerializer : ISwaggerSerializer
{
    public string Serialize(object instance)
    {
        var settings = new JsonSerializerSettings()
        {
            ContractResolver = new DefaultContractResolver(),
            TypeNameHandling = TypeNameHandling.Auto
        };
        return JsonConvert.SerializeObject(instance, settings);
    }

    public T Deserialize<T>(string jsonString)
    {
        var settings = new JsonSerializerSettings()
        {
            ContractResolver = new DefaultContractResolver(),
            TypeNameHandling = TypeNameHandling.Auto
        };
        return JsonConvert.DeserializeObject<T>(jsonString, settings);
    }
}