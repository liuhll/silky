using System;
using Silky.Core.DependencyInjection;

namespace Silky.Caching
{
    public interface IDistributedCacheSerializer
    {
        byte[] Serialize<T>(T obj);

        T Deserialize<T>(byte[] bytes);

        object Deserialize(byte[] bytes, Type type);
    }
}