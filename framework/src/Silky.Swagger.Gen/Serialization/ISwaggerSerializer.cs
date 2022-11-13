namespace Silky.Swagger.Gen.Serialization;

public interface ISwaggerSerializer
{
    string Serialize(object instance);

    T Deserialize<T>(string jsonString);
}