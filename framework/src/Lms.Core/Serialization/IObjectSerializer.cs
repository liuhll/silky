using System;

namespace Lms.Core.Serialization
{
    public interface IObjectSerializer
    {
        byte[] Serialize<T>(T obj);

        T Deserialize<T>(byte[] bytes);

        object DeserializeObject(byte[] bytes);

    }
}