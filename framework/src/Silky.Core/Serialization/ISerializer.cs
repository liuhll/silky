using System;

namespace Silky.Core.Serialization
{
    public interface ISerializer
    {
        string Serialize(object instance, bool camelCase = true, bool indented = false);

        T Deserialize<T>(string jsonString, bool camelCase = true);

        object Deserialize(Type type, string jsonString, bool camelCase = true);
    }
}