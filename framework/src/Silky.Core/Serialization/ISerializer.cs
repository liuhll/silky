using System;
using Newtonsoft.Json;

namespace Silky.Core.Serialization
{
    public interface ISerializer
    {
        string Serialize(object instance, bool camelCase = true, bool indented = false,
            TypeNameHandling typeNameHandling = TypeNameHandling.None);

        T Deserialize<T>(string jsonString, bool camelCase = true,
            TypeNameHandling typeNameHandling = TypeNameHandling.None);

        object Deserialize(Type type, string jsonString, bool camelCase = true,
            TypeNameHandling typeNameHandling = TypeNameHandling.None);
    }
}